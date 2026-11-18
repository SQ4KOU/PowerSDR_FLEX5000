//=================================================================
// YouTube.cs
// Written by KE9NS and the help of copilot and Gemini 
// for Live Streaming PowerSDR to YouTube
// one click live streaming from PowerSDR to YouTube Studio OR
// optional OAuth 2.0 authentication to allow PowerSDR to create and bind a new broadcast to your existing YouTube stream key
// this allows you to live stream without having to manually click "Go Live" in YouTube Studio.  PowerSDR will do it for you automatically when you start streaming, and will end the broadcast when you stop streaming.
// requires client_secrets.json file from Google Cloud Console for OAuth 2.0 authentication
//
// go to console.cloud.google.com and create a new project, then enable the YouTube Data API v3, then create OAuth 2.0 credentials for a desktop app, then download the JSON file and save it as client_secrets.json in the same folder as PowerSDR.exe
// console.cloud.google.com -> APIs & Services -> Library -> YouTube Data API v3 -> Enable
// console.cloud.google.com -> APIs & Services -> OAuth consent screen -> fill in required fields and save
// console.cloud.google.com -> APIs & Services -> Credentials -> Create Credentials -> OAuth client ID -> Desktop App -> Create
// then click Download JSON and save it as client_secrets.json in the same folder as PowerSDR.exe
//
// also requires ffmpeg.exe in the same folder as PowerSDR.exe
// also requires NAudio.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.YouTube.v3.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.Auth.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.Core.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.Util.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.OAuth2.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.PlatformServices.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.Auth.PlatformServices.dll in the same folder as PowerSDR.exe
// also requires Google.Apis.Auth.PlatformServices.dll in the same folder as PowerSDR.exe
// use NuGet to install the following packages:
// Google.Apis.YouTube.v3
// Google.Apis.Auth
// Google.Apis.Core
// Google.Apis
// Google.Apis.Util
// Google.Apis.OAuth2
// Google.Apis.PlatformServices
// Google.Apis.Auth.PlatformServices
// NAudio
//
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: ke9ns@ke9ns.com
//=================================================================

using Microsoft.JScript;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using Google.Apis.Auth.OAuth2;              //.14c for YouTube API authentication
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace PowerSDR
{
    public class YouTubeStreamer
    {
        // used for capturing the entire console window to a bitmap for streaming
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const int SRCCOPY = 0x00CC0020;

        private PictureBox _targetDisplay;
        private string _rtmpUrl;
        private string _streamKey;
        private Thread _streamThread;
        private string _YouTubeAudio;

        private volatile bool _isStreaming = false;

        private Process _ffmpegProcess;
    
        private int _fps = 30;                  // YouTube recommends 30 fps for live streaming
        private int _width = 0;
        private int _height = 0;

        public static Console console;          // ke9ns mod  to allow console to pass back values to setup screen
        public Setup setupForm;                 // ke9ns communications with setupform  (i.e. allow combometertype.text update from inside console.cs) 


        private YouTubeService _youtubeService;     //.14c
        private string _activeBroadcastId;
        private string _boundStreamId;
        private volatile bool _stopRequested = false;


        //=================================================================
        public YouTubeStreamer(PictureBox targetDisplay, string rtmpUrl, string streamKey, string audio) //called from console.cs to start the streaming thread
        {
           
            _targetDisplay = targetDisplay;
            _rtmpUrl = rtmpUrl;
            _streamKey = streamKey;
            _YouTubeAudio = audio;
        }

        string YouTubeType = "Public";  //ke9ns added to allow user to select public or unlisted for YouTube streaming

        bool oauth = false;     //ke9ns added to select either manual streaming false, or OAuth 2.0 streaming true.  If false, user must manually click "Go Live" in YouTube Studio.  If true, PowerSDR will automatically create and bind a new broadcast to your existing stream key and start streaming when you click Start in PowerSDR.

        //=================================================================
        public void Start()
        {
            if (_isStreaming) return;

            if (console.setupForm.chkYouTubePublic.Checked == true) 
            {
                YouTubeType = "public";  //ke9ns added to allow user to select public or unlisted for YouTube streaming
            }
            else
            {
                YouTubeType = "unlisted";  //ke9ns added to allow user to select public or unlisted for YouTube streaming
            }


            if (console.setupForm.chkYouTubeFull.Checked == true)
            {
                _width = console.Width;
                _height = console.Height;
            }
            else
            {
                _width = _targetDisplay.Width;
                _height = _targetDisplay.Height;
            }

            _width = _width & ~1;                           // FFmpeg H.264 encoder requires dimensions to be even numbers
            _height = _height & ~1;

            if (_width <= 0 || _height <= 0) return;

            string secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json");

            Debug.WriteLine("YOUTUBE client_secrets.json path: " + secretsPath);

            if (!File.Exists(secretsPath))
            {
                // Fallback Mode: No credentials file found. 
                // Expects user to manually click "Go Live" at studio.youtube.com
                Debug.WriteLine("YOUTUBE client_secrets.json not found. Falling back to manual Studio streaming mode.");
                oauth = false;
            }
            else
            {
                oauth = true;
                Debug.WriteLine("YOUTUBE client_secrets.json found. Live streaming enabled.");
            }

            _isStreaming = true;
            _streamThread = new Thread(StreamingLoop);
            _streamThread.Name = "YouTube Streaming Thread";
            _streamThread.IsBackground = true;  
            _streamThread.Priority = ThreadPriority.AboveNormal;
            _streamThread.Start();

            Debug.WriteLine("YOUTUBE STARTED...........................");

        } //Start()

        public void Stop()
        {
            _isStreaming = false;
            if (_streamThread != null && _streamThread.IsAlive)
            {
                _streamThread.Join(2000);
                
            }

            Debug.WriteLine("YOUTUBE STOPPED...........................");

        } //Stop()


    
        //=================================================================
        //Thread method for streaming loop
        //=================================================================
        private void StreamingLoop()
        {
        
            try
            {

                string fullRtmpUrl = $"{_rtmpUrl}/{_streamKey}"; // get from setup form


                if (oauth)
                {
                    CreateAndBindBroadcastForKey(
                        "client_secrets.json",                      // user needs to create one for his google account
                        _streamKey,                                 // studio.youtube.com and get it
                        "PowerSDR ke9ns v3 Live Stream",
                        "Live amateur radio from: " + console.SpotForm.callBox.Text,
                        YouTubeType // "public" or "unlisted" for testing
                    );
                }

                string speakerLoopbackDevice = _YouTubeAudio;       // setup->general->User Interface->YouTube Audio Device combobox value, e.g. "Speakers (Realtek(R) Audio)" or "CABLE Input (VB-Audio Virtual Cable)"

                string ffmpegArgs =
                        $"-y -use_wallclock_as_timestamps 1 -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {_width}x{_height} -r {_fps} -i pipe:0 " +
                        $"-rtbufsize 100M -f dshow -audio_buffer_size 50 -i audio=\"{speakerLoopbackDevice}\" " +
                        $"-c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2500k -maxrate 2500k -bufsize 5000k " +
                        $"-g {_fps * 2} -keyint_min {_fps} " +
                        $"-c:a aac -b:a 128k -af \"aresample=async=1\" " +
                        $"-f flv \"{fullRtmpUrl}\"";

               
                ProcessStartInfo psi = new ProcessStartInfo     // setup ffmpeg.exe process to stream to youtube
                {
                    FileName = "ffmpeg.exe",
                    Arguments = ffmpegArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true // diagnostics 
                };

                _ffmpegProcess = Process.Start(psi);            // startup ffmpeg.exe to stream to youtube using the above command line args

                string ffmpegsize = "Kib";                      // used for bottom right corner of console to show streaming size, bit rate and fps
                string ffmpegbit = "kbit";
                string ffmpegfps = "fps";

                _ffmpegProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.WriteLine("FFmpeg Log: " + args.Data);
                      
                        if (args.Data.Contains("frame= ") == true)
                        {
                            int loc = args.Data.IndexOf("fps= "); //
                            ffmpegfps= args.Data.Substring(loc + 5, 2);
                            loc = args.Data.IndexOf("size= "); //
                            ffmpegsize = args.Data.Substring(loc + 5, 11);
                            loc = args.Data.IndexOf("bitrate=");
                            ffmpegbit = args.Data.Substring(loc + 8, 11);

                            console.txtDisplayPeakOffset.Text = ffmpegsize;       // send to bottom right side corner of console screen to let you know your streaming
                            console.txtDisplayPeakPower.Text = ffmpegbit;
                            console.txtDisplayPeakFreq.Text = ffmpegfps + "fps";

                        }
                    }
                }; //  _ffmpegProcess.ErrorDataReceived

                _ffmpegProcess.BeginErrorReadLine();
             
                Stream ffmpegStdin = _ffmpegProcess.StandardInput.BaseStream;

              
                byte[] videoBuffer = null;

           
                Debug.WriteLine("YOUTUBE STREAMING...........................");

                System.Diagnostics.Stopwatch frameTimer = System.Diagnostics.Stopwatch.StartNew();
                double targetFrameMs = 1000.0 / _fps;
                long nextFrameTimeMs = 0;
                //----------------------------------------------------------------------------------------
                //----------------------------------------------------------------------------------------
                // main streaming loop part of thread
                //----------------------------------------------------------------------------------------
                //----------------------------------------------------------------------------------------
                while (_isStreaming && !_ffmpegProcess.HasExited)
                {
                    long nowMs = frameTimer.ElapsedMilliseconds;

                    // this is to sync up the audio and video streams to avoid drift.  If we are ahead of schedule, sleep until it's time for the next frame.  If we are behind schedule, skip frames to catch up.
                    if (nowMs < nextFrameTimeMs)                            // if we are ahead of schedule, sleep until it's time for the next frame
                    {
                        int sleepMs = (int)(nextFrameTimeMs - nowMs);       // calculate how long to sleep to maintain target frame rate

                        if (sleepMs > 1) Thread.Sleep(sleepMs - 1);         // sleep a bit less than the target frame time to avoid overshooting

                        continue;
                    }
                    nextFrameTimeMs += (long)targetFrameMs;


                    Bitmap bmp = null;

                    if (console.setupForm.chkYouTubeFull.Checked == true)
                    {
                        bmp = CaptureConsoleWindow(); // capture the entire console window to bitmap for streaming
                    }
                    else
                    {
                       bmp = CapturePictureBox(); // capture the target picDisplay to bitmap for streaming
                    }

                    if (bmp != null)
                    {

                        int byteCount = _width * _height * 3;
                        if (videoBuffer == null || videoBuffer.Length != byteCount)
                        {
                            videoBuffer = new byte[byteCount];
                        }

                        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, _width, _height),ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);

                        int bytesPerPixel = 3;
                        int targetStride = _width * bytesPerPixel;


                        if (bmpData.Stride == targetStride)    // If stride matches target width, copy all at once. Otherwise, copy row by row to strip padding.
                        {
                            int totalBytes = bmpData.Stride * _height;
                            if (videoBuffer == null || videoBuffer.Length != totalBytes)
                            {
                                videoBuffer = new byte[totalBytes];
                            }
                            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, videoBuffer, 0, totalBytes);
                        }
                        else
                        {
                            int totalBytes = targetStride * _height;
                            if (videoBuffer == null || videoBuffer.Length != totalBytes)
                            {
                                videoBuffer = new byte[totalBytes];
                            }

                            IntPtr ptr = bmpData.Scan0;
                            for (int y = 0; y < _height; y++)
                            {
                                System.Runtime.InteropServices.Marshal.Copy(ptr, videoBuffer, y * targetStride, targetStride);
                                ptr = IntPtr.Add(ptr, bmpData.Stride);
                            }
                        }

                        bmp.UnlockBits(bmpData); // Unlock the bitmap data after copying to videoBuffer   
                        bmp.Dispose();
                      
                        ffmpegStdin.Write(videoBuffer, 0, videoBuffer.Length);  // Push corrected bytes to FFmpeg stdin
                        ffmpegStdin.Flush();                                    // Flush to ensure timely delivery to FFmpeg

                    } //bmp != null

                  //  Thread.Sleep(10);  // .14 ke9ns added to reduce CPU usage and allow other threads to run.  This is a bit of a hack, but it works.  The frame pacing is handled by FFmpeg, so we don't need to worry about it here. 
                 
                } //while (_isStreaming && !_ffmpegProcess.HasExited)

                Debug.WriteLine("YOUTUBE STREAMING done...........................");
              
            }
            catch (Exception ex)
            {
                Debug.WriteLine("YouTubeStreamer Error: " + ex.Message);
            }
            finally
            {
               
                Cleanup();
                if (oauth)
                {
                    EndBroadcast();
                }

                Debug.WriteLine("YOUTUBE Cleanup and END BROADCAST...........................");

            } // finally
        } //SteamingLoop

        //--------------------------------------------------------------
        // Capture picDisplay PictureBox to Bitmap
        private Bitmap CapturePictureBox()
        {
            _width = _targetDisplay.Width;
            _height = _targetDisplay.Height;
            _width = _width & ~1;                   // FFmpeg H.264 encoder requires dimensions to be even numbers
            _height = _height & ~1;

            Bitmap bmp = null;
            if (_width > 0 && _height > 0)
            {
                try
                {
                    if (_width > 0 && _height > 0)
                    {
                        bmp = new Bitmap(_width, _height);

                        _targetDisplay.DrawToBitmap(bmp, new Rectangle(0, 0, _width, _height));
                    }
                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("YOUTUBE CAPTURE EXCEPTION: " + ex.Message);
                }
            } //if (_width > 0 && _height > 0)

            return bmp;
        } //CapturePictureBox()

        //--------------------------------------------------------------
        // Capture the entire console window to Bitmap
        private Bitmap CaptureConsoleWindow()
        {
            Bitmap bmp = null;
          
            _width = console.Width;
            _height = console.Height;
            _width = _width & ~1;   // FFmpeg H.264 encoder requires dimensions to be even numbers
            _height = _height & ~1;

            if (_width > 0 && _height > 0)
            {
                bmp = new Bitmap(_width, _height);
                using (Graphics g = Graphics.FromImage(bmp)) // create a graphics object from the bitmap to draw the console window onto it
                {
                    IntPtr hdcSrc = GetWindowDC(console.Handle); // get the device context of the console window
                    IntPtr hdcDest = g.GetHdc();    // get the device context of the bitmap
                    try
                    {
                        BitBlt(hdcDest, 0, 0, _width, _height, hdcSrc, 0, 0, SRCCOPY); // bitblt the console window onto the bitmap
                    }
                    finally
                    {
                        g.ReleaseHdc(hdcDest);
                        ReleaseDC(console.Handle, hdcSrc); //
                    }
                } // using (Graphics g = Graphics.FromImage(bmp))

            } //if (_width > 0 && _height > 0)

            return bmp;
   
        }//CaptureConsoleWindow()

        private void Cleanup() // called when stopping streaming to clean up FFmpeg process
        {
            try
            {
                if (_ffmpegProcess != null)
                {
                    if (!_ffmpegProcess.HasExited)
                    {
                        try
                        {
                            // Close stdin and send 'q' signal so FFmpeg flushes the FLV stream trailer cleanly
                            _ffmpegProcess.StandardInput.Write("q");
                            _ffmpegProcess.StandardInput.Flush();
                            _ffmpegProcess.StandardInput.Close();
                        }
                        catch { }

                        // Give FFmpeg 500ms to finish flushing, then force-kill so dshow audio doesn't keep it alive
                        if (!_ffmpegProcess.WaitForExit(500))
                        {
                            _ffmpegProcess.Kill();
                        }
                    }

                    _ffmpegProcess.Dispose();
                    _ffmpegProcess = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FFmpeg Cleanup Error: " + ex.Message);
            }
        } //Cleanup()

        //=================================================================
        // Authenticates via OAuth 2.0, finds the YouTube LiveStream matching your setup form's
        // _streamKey, creates a new broadcast, and binds them together.

        // This also requires you to go to console.cloud.google.com
        // APIs & Services -> Library, search for YouTube Data API v3, and click Enable.
        // then Go to APIs & Services -> OAuth consent screen: fill in required fields and save.
        // Then go to APIs & Services -> Credentials, click Create Credentials -> OAuth client ID -> Desktop App -> Create.
        // Then click Download JSON and save it as client_secrets.json in the same folder as PowerSDR.exe.
        // 
        //=================================================================
        public void CreateAndBindBroadcastForKey(string clientSecretsPath, string streamKey, string title, string description, string privacyStatus)
        {
            UserCredential credential;
            using (var stream = new FileStream(clientSecretsPath, FileMode.Open, FileAccess.Read))
            {
                string credPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PowerSDR_YouTubeAuth");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "PowerSDR-ke9ns"
            });

            var streamRequest = _youtubeService.LiveStreams.List("id,cdn,status");  // Find the YouTube LiveStream resource that matches your _streamKey (entered in setup->general->user interface->YouTube Stream Key)
            streamRequest.Mine = true;
            var streamResponse = streamRequest.Execute();

            _boundStreamId = null;
            if (streamResponse.Items != null)
            {
                foreach (var item in streamResponse.Items) // for loop through the user's live streams to find the one that matches the provided stream key
                {
                    if (item.Cdn?.IngestionInfo?.StreamName == streamKey) // Check if the stream name matches the provided stream key
                    {
                        _boundStreamId = item.Id;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(_boundStreamId))
            {
                throw new Exception("The Stream Key entered in PowerSDR Setup was not found on this YouTube account.");
            }

           
            var broadcast = new LiveBroadcast    // Create the broadcast and bind it to your existing _streamKey
            {
                Snippet = new LiveBroadcastSnippet
                {
                    Title = title,
                    Description = description,
                    ScheduledStartTimeDateTimeOffset = DateTimeOffset.UtcNow
                },
                Status = new LiveBroadcastStatus
                {
                    PrivacyStatus = privacyStatus, // "public", "unlisted", or "private"
                    SelfDeclaredMadeForKids = false
                },
                ContentDetails = new LiveBroadcastContentDetails
                {
                    EnableAutoStart = true,
                    EnableAutoStop = true
                }
            };

            var createdBroadcast = _youtubeService.LiveBroadcasts
                .Insert(broadcast, "snippet,status,contentDetails")
                .Execute();

            _activeBroadcastId = createdBroadcast.Id;

            var bindRequest = _youtubeService.LiveBroadcasts.Bind(_activeBroadcastId, "id,contentDetails");
            bindRequest.StreamId = _boundStreamId;
            bindRequest.Execute();

        } //CreateAndBindBroadcastForKey


        //=================================================================
        // Call this when stopping FFmpeg to end the broadcast cleanly.
        //=================================================================
        public void EndBroadcast()
        {
            _stopRequested = true;

            if (_youtubeService == null || string.IsNullOrEmpty(_activeBroadcastId))
                return;

            try
            {
                _youtubeService.LiveBroadcasts.Transition(
                    LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Complete,
                    _activeBroadcastId,
                    "status").Execute();
            }
            catch
            {
                // Ignore if already ended via EnableAutoStop
            }
            finally
            {
                _activeBroadcastId = null;
            }
        } // EndBroadcast
    } // Youtubestream class


      
        /*
                            if (!_hasSavedDebugImage)
                            {
                                try
                                {
                                    string debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_stream_frame.bmp");
                                    bmp.Save(debugPath, ImageFormat.Bmp);
                                    _hasSavedDebugImage = true;
                                    Debug.WriteLine("YOUTUBE DEBUG: Saved snapshot to " + debugPath);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("YOUTUBE DEBUG SAVE ERROR: " + ex.Message);
                                }
                            }

    */

    } //PowerSDR namespace