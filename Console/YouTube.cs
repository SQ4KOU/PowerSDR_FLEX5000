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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace PowerSDR
{
    public class YouTubeStreamer
    {
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);
        private const uint PW_RENDERFULLCONTENT = 0x00000002; // Flag constants:
                                                              // 0x0 = Captures the entire window (title bar, borders, client area)
                                                              // 0x1 (PW_CLIENTONLY) = Captures only the client drawing area (excluding borders/title bar)

        private PictureBox _targetDisplay;
        private string _rtmpUrl;
        private string _streamKey;
        private Thread _streamThread;
        private string _YouTubeAudio;

        private volatile bool _isStreaming = false;

        private Process _ffmpegProcess;
     //   private WasapiLoopbackCapture _audioCapture;  
     //   private BufferedWaveProvider _audioProvider;
     //   private bool _hasSavedDebugImage = false; // 

        private int _fps = 30;
        private int _width = 0;
        private int _height = 0;

        public static Console console;          //.14 ke9ns mod  to allow console to pass back values to setup screen
        public Setup setupForm;                 //.14 ke9ns communications with setupform  (i.e. allow combometertype.text update from inside console.cs) 

       
        public YouTubeStreamer(PictureBox targetDisplay, string rtmpUrl, string streamKey, string audio)
        {
           
            _targetDisplay = targetDisplay;
            _rtmpUrl = rtmpUrl;
            _streamKey = streamKey;
            _YouTubeAudio = audio;
        }

        public void Start()
        {
            if (_isStreaming) return;

            // Ensure dimensions are fetched safely from the UI thread
            _targetDisplay.Invoke((MethodInvoker)delegate {
                _width = _targetDisplay.Width;
                _height = _targetDisplay.Height;
                // FFmpeg H.264 encoder requires dimensions to be even numbers
                _width = _width & ~1;
                _height = _height & ~1;
            });

            if (_width <= 0 || _height <= 0) return;

            _isStreaming = true;
            _streamThread = new Thread(StreamingLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            _streamThread.Start();

            Debug.WriteLine("YOUTUBE STARTED...........................");

        }

        public void Stop()
        {
            _isStreaming = false;
            if (_streamThread != null && _streamThread.IsAlive)
            {
                _streamThread.Join(2000);
            }

            Debug.WriteLine("YOUTUBE STOPPED...........................");

        }

        private void StreamingLoop()
        {
         
            try
            {

                string fullRtmpUrl = $"{_rtmpUrl}/{_streamKey}";

                string speakerLoopbackDevice = _YouTubeAudio; //.14

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
                // mix 2 audio channels togther
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
                    RedirectStandardError = true // Crucial for diagnosing silent failures
                };

                _ffmpegProcess = Process.Start(psi);

                string ffmpegsize = "Kib";
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

                int frameIntervalMs = 1000 / _fps;
                byte[] videoBuffer = null;


                //==================================================================================
                Debug.WriteLine("YOUTUBE STREAMING...........................");

                // 3. Main Streaming Frame Loop
                while (_isStreaming && !_ffmpegProcess.HasExited)
                {
                    long loopStart = DateTime.UtcNow.Ticks;

                    // Capture frame from PictureBox safely via Invoke

                    Bitmap bmp = null;

                    if (console.setupForm.chkYouTubeFull.Checked == true)
                    {
                        bmp = CaptureConsoleWindow();
                    }
                    else
                    {
                      //  Debug.WriteLine("YOUTUBE pan");
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

                    // Maintain steady 30 FPS frame pacing
                    long elapsedMs = (DateTime.UtcNow.Ticks - loopStart) / TimeSpan.TicksPerMillisecond;
                    int sleepTime = frameIntervalMs - (int)elapsedMs;
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
                    }
                }
                Debug.WriteLine("YOUTUBE STREAMING done...........................");
              
            }
            catch (Exception ex)
            {
                Debug.WriteLine("YouTubeStreamer Error: " + ex.Message);
            }
            finally
            {
                Cleanup();
            }
        } //SteamingLoop

      
        private Bitmap CapturePictureBox()
        {
            Bitmap bmp = null;

                        try
                        {
                            _targetDisplay.Invoke((MethodInvoker)delegate {
                                if (_width > 0 && _height > 0)
                                {
                                   bmp = new Bitmap(_width, _height);

                                    _targetDisplay.DrawToBitmap(bmp, new Rectangle(0, 0, _width, _height));
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("YOUTUBE CAPTURE EXCEPTION: " + ex.Message);
                        }

           

            return bmp;
        } //CapturePictureBox()

        private Bitmap CaptureConsoleWindow()
        {
            Bitmap bmp = null;
            int w = console.Width;
            int h = console.Height;

            if (w > 0 && h > 0)
            {
                bmp = new Bitmap(w, h);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    IntPtr hdc = g.GetHdc();
                    try
                    {
                       
                        PrintWindow(console.Handle, hdc, PW_RENDERFULLCONTENT); // Call native PrintWindow using the form's window handle
                    }
                    finally
                    {
                        g.ReleaseHdc(hdc);
                    }
                }
            }
            return bmp;


            /*
                        Bitmap bmp = null;
                        try
                        {
                                    // Assuming _consoleForm is a reference to your active console.cs instance
                                    console.Invoke(new Action(() =>
                                    {
                                        int w = console.Width;
                                        int h = console.Height;

                                        bmp = new Bitmap(w, h);
                                        console.DrawToBitmap(bmp, new Rectangle(0, 0, w, h));
                                    }));
                                }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Console Capture Error: " + ex.Message);
                        }
                        return bmp;
            */
            /*
                        Graphics myGraphics = this.CreateGraphics(); // graphics control for the full PSDR console
                        Size s = this.Size;                          // current size of the PSDR console

                        bitmap = new Bitmap(s.Width, s.Height, myGraphics);                                 // create bitmap the exact size of the full console screen
                        Graphics memGraph = Graphics.FromImage(bitmap);                                     // make graphic the size of the PSDR console
                        memGraph.CopyFromScreen(this.Location.X, this.Location.Y, 0, 0, s);                 // copy the full console screen to memGraph

                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);                           // create JPEG format
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;  // create JPEG encoder
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);                   // parameters
                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 99L);         // 99% quality  (reduction to reduce byte size since this is the entire console screen)
                        myEncoderParameters.Param[0] = myEncoderParameter;                                  // set paramaters

                        bitmap.Save(file_name, jpgEncoder, myEncoderParameters);                            // save FILE as JPEG using Parameters

                        */
        }//CaptureConsoleWindow()


        private void Cleanup()
        {
            try
            {
             //   if (_audioCapture != null)
              //  {
               //     _audioCapture.StopRecording();
                //    _audioCapture.Dispose();
                 //   _audioCapture = null;
              //  }
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.StandardInput.Close();
                    _ffmpegProcess.WaitForExit(1000);
                    _ffmpegProcess.Dispose();
                    _ffmpegProcess = null;
                }
            }
            catch { }
        }
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