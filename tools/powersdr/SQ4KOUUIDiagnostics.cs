using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PowerSDR
{
    internal static class SQ4KOUUIDiagnostics
    {
        private static readonly Stopwatch Clock = Stopwatch.StartNew();
        private static readonly ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();
        private static readonly AutoResetEvent Wake = new AutoResetEvent(false);
        private static readonly object InitSync = new object();

        private static Thread writerThread;
        private static Thread watchdogThread;
        private static volatile bool stopping;
        private static volatile bool started;
        private static volatile bool uiAttached;
        private static Control uiRoot;
        private static int uiThreadId;
        private static int pingPending;
        private static long lastUiAckTicks = Stopwatch.GetTimestamp();
        private static long nextSummaryTicks;
        private static int lastStallLevel;
        private static string lastUiMessage = "none";
        private static DiagMessageFilter messageFilter;

        private static long invalidateRequests;
        private static long producerTicks;
        private static long paintCount;
        private static long paintTicksTotal;
        private static long paintTicksMax;
        private static long renderCount;
        private static long renderTicksTotal;
        private static long renderTicksMax;

        private static string logPath;
        private static string latestPath;

        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr hProcess, int uiFlags);

        internal static string LogPath
        {
            get { return logPath ?? String.Empty; }
        }

        internal static void StartEarly()
        {
            if (started) return;
            lock (InitSync)
            {
                if (started) return;

                string root = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FlexRadio Systems",
                    "PowerSDR v2.8.0",
                    "SQ4KOU Diagnostics");

                Directory.CreateDirectory(root);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                logPath = Path.Combine(root, "SQ4KOU_UI_DIAG_" + stamp + ".log");
                latestPath = Path.Combine(root, "SQ4KOU_UI_DIAG_LATEST.txt");

                try
                {
                    File.WriteAllText(latestPath, logPath + Environment.NewLine, Encoding.UTF8);
                }
                catch { }

                stopping = false;
                writerThread = new Thread(WriterLoop);
                writerThread.Name = "SQ4KOU UI Diagnostics Writer";
                writerThread.IsBackground = true;
                writerThread.Priority = ThreadPriority.BelowNormal;
                writerThread.Start();

                started = true;
                Mark("DIAG", "START", "version=P17;log=" + logPath);
                InstallExceptionHooks();
            }
        }

        internal static void AttachUI(Control root)
        {
            StartEarly();
            if (root == null) return;

            uiRoot = root;
            uiThreadId = Thread.CurrentThread.ManagedThreadId;
            uiAttached = true;
            lastUiAckTicks = Stopwatch.GetTimestamp();

            if (messageFilter == null)
            {
                messageFilter = new DiagMessageFilter();
                Application.AddMessageFilter(messageFilter);
            }

            Form form = root as Form;
            if (form != null)
            {
                form.Shown += delegate
                {
                    Mark("STARTUP", "FORM_SHOWN", FormState(form));
                    StartWatchdog();
                };
                form.FormClosed += delegate
                {
                    Mark("STARTUP", "FORM_CLOSED", FormState(form));
                    RequestStop();
                };
            }

            Mark("UI", "ATTACHED", "ui_tid=" + uiThreadId + ";" + FormState(root as Form));
        }

        private static void InstallExceptionHooks()
        {
            try
            {
                Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
                {
                    Mark("EXCEPTION", "UI_THREAD", ExceptionText(e.Exception));
                };
            }
            catch { }

            try
            {
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
                {
                    Exception ex = e.ExceptionObject as Exception;
                    Mark("EXCEPTION", "APPDOMAIN", ExceptionText(ex) + ";terminating=" + e.IsTerminating);
                };
            }
            catch { }
        }

        private static string ExceptionText(Exception ex)
        {
            if (ex == null) return "null";
            return ex.GetType().FullName + ":" + ex.Message + "|" + ex.StackTrace;
        }

        private static string FormState(Form f)
        {
            if (f == null) return "form=null";
            try
            {
                return "state=" + f.WindowState +
                    ";bounds=" + f.Bounds.X + "," + f.Bounds.Y + "," + f.Bounds.Width + "," + f.Bounds.Height +
                    ";visible=" + f.Visible;
            }
            catch
            {
                return "form_state_unavailable";
            }
        }

        private static void StartWatchdog()
        {
            if (watchdogThread != null) return;
            lock (InitSync)
            {
                if (watchdogThread != null) return;

                watchdogThread = new Thread(WatchdogLoop);
                watchdogThread.Name = "SQ4KOU UI Diagnostics Watchdog";
                watchdogThread.IsBackground = true;
                watchdogThread.Priority = ThreadPriority.BelowNormal;
                watchdogThread.Start();
                Mark("UI", "WATCHDOG_START", null);
            }
        }

        internal static long OperationBegin(string area, string operation, string detail)
        {
            StartEarly();
            long t = Stopwatch.GetTimestamp();
            Mark(area, operation + "_BEGIN", detail);
            return t;
        }

        internal static void OperationEnd(long startTicks, string area, string operation, string detail)
        {
            double ms = TicksToMs(Stopwatch.GetTimestamp() - startTicks);
            Mark(area, operation + "_END", "duration_ms=" + ms.ToString("F3") + AppendDetail(detail));
        }

        internal static void Mark(string area, string evt, string detail)
        {
            StartEarly();

            long elapsed = Clock.ElapsedMilliseconds;
            int tid = Thread.CurrentThread.ManagedThreadId;
            string line =
                elapsed.ToString() + "|" +
                DateTime.UtcNow.ToString("o") + "|" +
                tid.ToString() + "|" +
                ((uiThreadId != 0 && tid == uiThreadId) ? "UI" : "BG") + "|" +
                Clean(area) + "|" +
                Clean(evt) + "|" +
                Clean(detail);

            Queue.Enqueue(line);
            Wake.Set();
        }

        internal static void InvalidateRequested()
        {
            Interlocked.Increment(ref invalidateRequests);
        }

        internal static void ProducerTick()
        {
            Interlocked.Increment(ref producerTicks);
        }

        internal static long PaintBegin()
        {
            return Stopwatch.GetTimestamp();
        }

        internal static void PaintEnd(long startTicks)
        {
            long dt = Stopwatch.GetTimestamp() - startTicks;
            Interlocked.Increment(ref paintCount);
            Interlocked.Add(ref paintTicksTotal, dt);
            UpdateMax(ref paintTicksMax, dt);

            double ms = TicksToMs(dt);
            if (ms >= 25.0)
                Mark("DISPLAY", "SLOW_PAINT", "duration_ms=" + ms.ToString("F3") + ";last_ui=" + lastUiMessage);
        }

        internal static long RenderBegin()
        {
            return Stopwatch.GetTimestamp();
        }

        internal static void RenderEnd(long startTicks)
        {
            long dt = Stopwatch.GetTimestamp() - startTicks;
            Interlocked.Increment(ref renderCount);
            Interlocked.Add(ref renderTicksTotal, dt);
            UpdateMax(ref renderTicksMax, dt);

            double ms = TicksToMs(dt);
            if (ms >= 20.0)
                Mark("DISPLAY", "SLOW_RENDER", "duration_ms=" + ms.ToString("F3") + ";last_ui=" + lastUiMessage);
        }

        private static void UpdateMax(ref long location, long value)
        {
            long observed;
            do
            {
                observed = Interlocked.Read(ref location);
                if (value <= observed) return;
            }
            while (Interlocked.CompareExchange(ref location, value, observed) != observed);
        }

        private static void WatchdogLoop()
        {
            nextSummaryTicks = Stopwatch.GetTimestamp();

            while (!stopping)
            {
                Thread.Sleep(100);

                Control root = uiRoot;
                if (uiAttached && root != null)
                {
                    if (Interlocked.CompareExchange(ref pingPending, 1, 0) == 0)
                    {
                        try
                        {
                            root.BeginInvoke(new MethodInvoker(UiPingAck));
                        }
                        catch
                        {
                            Interlocked.Exchange(ref pingPending, 0);
                        }
                    }

                    long now = Stopwatch.GetTimestamp();
                    double lagMs = TicksToMs(now - Interlocked.Read(ref lastUiAckTicks));
                    int stallLevel = StallLevel(lagMs);

                    if (stallLevel > 0 && stallLevel != lastStallLevel)
                    {
                        lastStallLevel = stallLevel;
                        Mark("UI", "STALL", "lag_ms=" + lagMs.ToString("F0") + ";level=" + stallLevel + ";last_ui=" + lastUiMessage);
                    }
                    else if (stallLevel == 0)
                    {
                        if (lastStallLevel > 0)
                            Mark("UI", "STALL_END", "lag_ms=" + lagMs.ToString("F0") + ";last_level=" + lastStallLevel + ";last_ui=" + lastUiMessage);
                        lastStallLevel = 0;
                    }

                    if (now >= nextSummaryTicks)
                    {
                        nextSummaryTicks = now + Stopwatch.Frequency;
                        EmitSummary(lagMs);
                    }
                }
            }
        }

        private static void UiPingAck()
        {
            Interlocked.Exchange(ref lastUiAckTicks, Stopwatch.GetTimestamp());
            Interlocked.Exchange(ref pingPending, 0);
        }

        private static int StallLevel(double ms)
        {
            if (ms >= 10000) return 6;
            if (ms >= 5000) return 5;
            if (ms >= 2000) return 4;
            if (ms >= 1000) return 3;
            if (ms >= 500) return 2;
            if (ms >= 250) return 1;
            return 0;
        }

        private static void EmitSummary(double uiLagMs)
        {
            long inv = Interlocked.Read(ref invalidateRequests);
            long prod = Interlocked.Read(ref producerTicks);
            long paints = Interlocked.Read(ref paintCount);
            long pTotal = Interlocked.Read(ref paintTicksTotal);
            long pMax = Interlocked.Read(ref paintTicksMax);
            long renders = Interlocked.Read(ref renderCount);
            long rTotal = Interlocked.Read(ref renderTicksTotal);
            long rMax = Interlocked.Read(ref renderTicksMax);

            double pAvg = paints > 0 ? TicksToMs(pTotal) / paints : 0.0;
            double rAvg = renders > 0 ? TicksToMs(rTotal) / renders : 0.0;

            int gdi = -1;
            int user = -1;
            long ws = 0;
            try
            {
                Process p = Process.GetCurrentProcess();
                gdi = GetGuiResources(p.Handle, 0);
                user = GetGuiResources(p.Handle, 1);
                ws = p.WorkingSet64;
            }
            catch { }

            Mark(
                "PERF",
                "SUMMARY",
                "ui_lag_ms=" + uiLagMs.ToString("F0") +
                ";producer=" + prod +
                ";invalidate=" + inv +
                ";paint=" + paints +
                ";paint_avg_ms=" + pAvg.ToString("F3") +
                ";paint_max_ms=" + TicksToMs(pMax).ToString("F3") +
                ";render=" + renders +
                ";render_avg_ms=" + rAvg.ToString("F3") +
                ";render_max_ms=" + TicksToMs(rMax).ToString("F3") +
                ";gdi=" + gdi +
                ";user=" + user +
                ";working_set_mb=" + (ws / 1048576L) +
                ";gc0=" + GC.CollectionCount(0) +
                ";gc1=" + GC.CollectionCount(1) +
                ";gc2=" + GC.CollectionCount(2) +
                ";last_ui=" + lastUiMessage);
        }

        internal static void NoteUiMessage(Message m)
        {
            int msg = m.Msg;
            if (!IsInterestingUiMessage(msg)) return;

            string name = String.Empty;
            try
            {
                Control c = Control.FromHandle(m.HWnd);
                if (c != null) name = c.Name;
            }
            catch { }

            lastUiMessage = "0x" + msg.ToString("X") + ":" + Clean(name);
        }

        private static bool IsInterestingUiMessage(int msg)
        {
            return msg == 0x0111 || // WM_COMMAND
                   msg == 0x004E || // WM_NOTIFY
                   msg == 0x0201 || // WM_LBUTTONDOWN
                   msg == 0x0202 || // WM_LBUTTONUP
                   msg == 0x0204 || // WM_RBUTTONDOWN
                   msg == 0x0205 || // WM_RBUTTONUP
                   msg == 0x0100 || // WM_KEYDOWN
                   msg == 0x0101;   // WM_KEYUP
        }

        private sealed class DiagMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                NoteUiMessage(m);
                return false;
            }
        }

        private static void WriterLoop()
        {
            StreamWriter writer = null;
            try
            {
                FileStream fs = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                writer = new StreamWriter(fs, new UTF8Encoding(false));
                writer.AutoFlush = false;
                writer.WriteLine("elapsed_ms|utc|tid|thread|area|event|detail");
                writer.Flush();

                while (!stopping || !Queue.IsEmpty)
                {
                    Wake.WaitOne(250);

                    string line;
                    bool wrote = false;
                    while (Queue.TryDequeue(out line))
                    {
                        writer.WriteLine(line);
                        wrote = true;
                    }

                    if (wrote) writer.Flush();
                }
            }
            catch
            {
            }
            finally
            {
                if (writer != null)
                {
                    try { writer.Flush(); } catch { }
                    try { writer.Dispose(); } catch { }
                }
            }
        }

        private static void RequestStop()
        {
            if (stopping) return;
            Mark("DIAG", "STOP_REQUEST", null);
            stopping = true;
            Wake.Set();
        }

        private static double TicksToMs(long ticks)
        {
            return (double)ticks * 1000.0 / Stopwatch.Frequency;
        }

        private static string AppendDetail(string detail)
        {
            string clean = Clean(detail);
            return clean.Length == 0 ? String.Empty : ";" + clean;
        }

        private static string Clean(string value)
        {
            if (String.IsNullOrEmpty(value)) return String.Empty;
            return value.Replace("\r", " ").Replace("\n", " ").Replace("|", "/");
        }
    }
}
