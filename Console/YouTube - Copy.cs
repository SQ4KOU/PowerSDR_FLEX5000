//=================================================================
// YouTube.cs
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
// You may contact us via email at: gpl@flexradio.com.
//=================================================================

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using NAudio.Wave;

namespace PowerSDR
{
    public class YouTubeStreamer
    {
        private PictureBox _targetDisplay;
        private string _rtmpUrl;
        private string _streamKey;
        private Thread _streamThread;
        private volatile bool _isStreaming = false;

        private Process _ffmpegProcess;
        private WasapiLoopbackCapture _audioCapture;
        private BufferedWaveProvider _audioProvider;

        private bool _hasSavedDebugImage = false; // 

        private int _fps = 30;
        private int _width = 0;
        private int _height = 0;

        public YouTubeStreamer(PictureBox targetDisplay, string rtmpUrl, string streamKey)
        {
            _targetDisplay = targetDisplay;
            _rtmpUrl = rtmpUrl;
            _streamKey = streamKey;
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
                // 1. Start NAudio WASAPI Loopback to capture default speaker/audio output
                _audioCapture = new WasapiLoopbackCapture();
                _audioProvider = new BufferedWaveProvider(_audioCapture.WaveFormat)
                {
                    DiscardOnBufferOverflow = true,
                    BufferLength = 1024 * 1024
                };

                _audioCapture.DataAvailable += (s, e) =>
                {
                    if (_isStreaming && _audioProvider != null)
                    {
                        _audioProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
                    }
                };
                _audioCapture.StartRecording();

                // 2. Start FFmpeg Process
             
                string fullRtmpUrl = $"{_rtmpUrl}/{_streamKey}";

                Debug.WriteLine("YOUTUBE key >" + fullRtmpUrl + "<");

                string ffmpegArgs =
    $"-y -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {_width}x{_height} -r {_fps} -i pipe:0 " +
    $"-f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 " +
    $"-c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2500k -maxrate 2500k -bufsize 5000k " +
    $"-g {_fps * 2} -keyint_min {_fps} " +
    $"-c:a aac -b:a 128k " +
    $"-shortest -f flv \"{fullRtmpUrl}\"";

                /*
                                string ffmpegArgs =
                    $"-y -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {_width}x{_height} -r {_fps} -i pipe:0 " +
                    $"-f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 " +
                    $"-c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2500k -maxrate 2500k -bufsize 5000k " +
                    $"-g {_fps * 2} -keyint_min {_fps} " +
                    $"-c:a aac -b:a 128k " +
                    $"-shortest -f flv \"{fullRtmpUrl}\"";

                */
                /* orginal
                      string ffmpegArgs =
                            $"-y -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {_width}x{_height} -r {_fps} -i pipe:0 " +
                            $"-f s16le -ar {_audioCapture.WaveFormat.SampleRate} -ac {_audioCapture.WaveFormat.Channels} -i pipe:1 " +
                            $"-c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2500k " +
                            $"-c:a aac -b:a 128k -f flv \"{fullRtmpUrl}\"";
                */


                /*
                           string ffmpegArgs = 
                    $"-y -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {_width}x{_height} -r {_fps} -i pipe:0 " +
                    $"-c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2500k -maxrate 2500k -bufsize 5000k " +
                    $"-g {_fps * 2} -keyint_min {_fps} -f flv \"{fullRtmpUrl}\"";     
                */

                /*
                                string ffmpegArgs =
                                    $"-y -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {_width}x{_height} -r {_fps} -i pipe:0 " +
                                    $"-c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2500k " +
                                    $"-f flv \"{fullRtmpUrl}\"";
                */

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

            
                _ffmpegProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.WriteLine("FFmpeg Log: " + args.Data);
                    }
                };
                _ffmpegProcess.BeginErrorReadLine();
               



                Stream ffmpegStdin = _ffmpegProcess.StandardInput.BaseStream;

                int frameIntervalMs = 1000 / _fps;
                byte[] videoBuffer = null;

                Debug.WriteLine("YOUTUBE STREAMING...........................");


                // 3. Main Streaming Frame Loop
                while (_isStreaming && !_ffmpegProcess.HasExited)
                {
                    long loopStart = DateTime.UtcNow.Ticks;

                    // Capture frame from PictureBox safely via Invoke
                    Bitmap bmp = CapturePictureBox();
                    if (bmp != null)
                    {


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

                        int byteCount = _width * _height * 3;
                        if (videoBuffer == null || videoBuffer.Length != byteCount)
                        {
                            videoBuffer = new byte[byteCount];
                        }




                        // Lock bitmap bits and copy raw BGR24 pixel bytes directly
                        BitmapData bmpData = bmp.LockBits(
                            new Rectangle(0, 0, _width, _height),
                            ImageLockMode.ReadOnly,
                            PixelFormat.Format24bppRgb);

                        System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, videoBuffer, 0, byteCount);
                        bmp.UnlockBits(bmpData);
                        bmp.Dispose();

                        // Push raw video frame to FFmpeg stdin
                        ffmpegStdin.Write(videoBuffer, 0, byteCount);
                        ffmpegStdin.Flush();
                    }

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
        }

        private Bitmap CapturePictureBox()
        {
            Bitmap bmp = null;
            try
            {
                _targetDisplay.Invoke((MethodInvoker)delegate {
                    if (_width > 0 && _height > 0)
                    {
                        // Create a blank bitmap matching the picture box dimensions
                        bmp = new Bitmap(_width, _height);

                        // Force the PictureBox control to render its current drawing surface into the bitmap
                        _targetDisplay.DrawToBitmap(bmp, new Rectangle(0, 0, _width, _height));
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("YOUTUBE CAPTURE EXCEPTION: " + ex.Message);
            }
            return bmp;
        }

        private void Cleanup()
        {
            try
            {
                if (_audioCapture != null)
                {
                    _audioCapture.StopRecording();
                    _audioCapture.Dispose();
                    _audioCapture = null;
                }
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

} //PowerSDR namespace