//=================================================================
// YouTube.cs
// Written by KE9nS
// for Live Streaming PowerSDR to YouTube
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
    
        private int _fps = 30;  // YouTube recommends 30 fps for live streaming
        private int _width = 0;
        private int _height = 0;

        public static Console console;          // ke9ns mod  to allow console to pass back values to setup screen
        public Setup setupForm;                 // ke9ns communications with setupform  (i.e. allow combometertype.text update from inside console.cs) 


        private YouTubeService _youtubeService;     //.14c
        private string _activeBroadcastId;
        private string _boundStreamId;
        private volatile bool _stopRequested = false;




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

            // Find the YouTube LiveStream resource that matches your setup form's _streamKey
            var streamRequest = _youtubeService.LiveStreams.List("id,cdn,status");
            streamRequest.Mine = true;
            var streamResponse = streamRequest.Execute();

            _boundStreamId = null;
            if (streamResponse.Items != null)
            {
                foreach (var item in streamResponse.Items)
                {
                    if (item.Cdn?.IngestionInfo?.StreamName == streamKey)
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

            // Create the broadcast and bind it to your existing _streamKey
            var broadcast = new LiveBroadcast
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

        //=================================================================
        public YouTubeStreamer(PictureBox targetDisplay, string rtmpUrl, string streamKey, string audio) //called from console.cs to start the streaming thread
        {
           
            _targetDisplay = targetDisplay;
            _rtmpUrl = rtmpUrl;
            _streamKey = streamKey;
            _YouTubeAudio = audio;
        }


        //=================================================================
        public void Start()
        {
            if (_isStreaming) return;

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

            _width = _width & ~1;   // FFmpeg H.264 encoder requires dimensions to be even numbers
            _height = _height & ~1;

            if (_width <= 0 || _height <= 0) return;
          
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


                CreateAndBindBroadcastForKey(
                    "client_secrets.json",
                    _streamKey,
                    "PowerSDR Live Stream",
                    "Live amateur radio waterfall and audio from PowerSDR ke9ns",
                    "public" // or "unlisted" for testing
                );


                string speakerLoopbackDevice = _YouTubeAudio; 

              //  Debug.WriteLine("YOUTUBE..." + _YouTubeAudio);

               //    string micDevice = "Microphone (Logi USB Headset)";                    
                //   string speakerLoopbackDevice = "CABLE Output (VB-Audio Virtual Cable)"; 

                //CABLE Output (VB-Audio Virtual Cable)
                //Microphone (Logi USB Headset)
                //CABLE Output (VB-Audio Virtual Cable)
                //CABLE-A Output (VB-Audio Virtual Cable A)
                //CABLE-C Output (VB-Audio Cable C)
                //CABLE-B Output (VB-Audio Virtual Cable B)
                //CABLE-D Output (VB-Audio Cable D)
                //Stereo Mix (Realtek(R) Audio)

/*
                // mix 2 audio channels togther (not used at this time)
               string ffmpegArgs =
                    $"-y -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {_width}x{_height} -r {_fps} -i pipe:0 " +
                    $"-f dshow -i audio=\"{micDevice}\" " +
                    $"-f dshow -i audio=\"{speakerLoopbackDevice}\" " +
                    $"-filter_complex \"[1:a][2:a]amix=inputs=2:duration=first[aout]\" " +
                    $"-c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2500k -maxrate 2500k -bufsize 5000k " +
                    $"-g {_fps * 2} -keyint_min {_fps} " +
                    $"-map 0:v:0 -map \"[aout]\" " +
                    $"-c:a aac -b:a 128k " +
                    $"-f flv \"{fullRtmpUrl}\"";

                */

                string ffmpegArgs =
                        $"-y -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {_width}x{_height} -r {_fps} -i pipe:0 " +
                        $"-f dshow -i audio=\"{speakerLoopbackDevice}\" " +
                        $"-c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2500k -maxrate 2500k -bufsize 5000k " +
                        $"-g {_fps * 2} -keyint_min {_fps} " +
                        $"-c:a aac -b:a 128k " +
                        $"-shortest -f flv \"{fullRtmpUrl}\"";


                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = ffmpegArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true // diagnostics 
                };

                _ffmpegProcess = Process.Start(psi);  // startup ffmpeg.exe to stream to youtube using the above command line args

                string ffmpegsize = "Kib";  // used for bottom right corner of console to show streaming size, bit rate and fps
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


                //-----------------------------------------------------------------
                // main streaming loop part of thread
                while (_isStreaming && !_ffmpegProcess.HasExited)
                {
                  
                    Bitmap bmp = null;

                    if (console.setupForm.chkYouTubeFull.Checked == true)
                    {
                        bmp = CaptureConsoleWindow(); //having issues with speed and performance when capturing the entire console window, so this is disabled for now.  It works, but it is slow and causes some stuttering in the video stream.  I will work on this later to see if I can improve the performance.  For now, just capture the picDisplay PictureBox instead.
                    }
                    else
                    {
                       bmp = CapturePictureBox();
                    }

                    if (bmp != null)
                    {

                        int byteCount = _width * _height * 3;
                        if (videoBuffer == null || videoBuffer.Length != byteCount)
                        {
                            videoBuffer = new byte[byteCount];
                        }

                        BitmapData bmpData = bmp.LockBits(
                          new Rectangle(0, 0, _width, _height),
                          ImageLockMode.ReadOnly,
                          PixelFormat.Format24bppRgb);

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

                        bmp.UnlockBits(bmpData);
                        bmp.Dispose();

                        // Push corrected bytes to FFmpeg stdin
                        ffmpegStdin.Write(videoBuffer, 0, videoBuffer.Length);
                        ffmpegStdin.Flush();


                    } //bmp != null

                    Thread.Sleep(10);  // .14 ke9ns added to reduce CPU usage and allow other threads to run.  This is a bit of a hack, but it works.  The frame pacing is handled by FFmpeg, so we don't need to worry about it here. 
                 
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
                EndBroadcast();
            }
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
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    IntPtr hdcSrc = GetWindowDC(console.Handle);
                    IntPtr hdcDest = g.GetHdc();
                    try
                    {
                        BitBlt(hdcDest, 0, 0, _width, _height, hdcSrc, 0, 0, SRCCOPY);
                    }
                    finally
                    {
                        g.ReleaseHdc(hdcDest);
                        ReleaseDC(console.Handle, hdcSrc);
                    }
                } // using (Graphics g = Graphics.FromImage(bmp))

            } //if (_width > 0 && _height > 0)

            return bmp;
   
        }//CaptureConsoleWindow()

        private void Cleanup()
        {
            try
            {
             
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.StandardInput.Close();
                    _ffmpegProcess.WaitForExit(1000);
                    _ffmpegProcess.Dispose();
                    _ffmpegProcess = null;
                }
            }
            catch { }
        }//Cleanup()


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