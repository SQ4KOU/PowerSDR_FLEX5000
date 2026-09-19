using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace PowerSDR
{
    internal static class SQ4KOUStartupPresentation
    {
        internal static void Install(Form form, Control display, Action afterReveal)
        {
            if (form == null || display == null) return;

            bool shown = false;
            bool firstDisplayPaintComplete = false;
            bool revealScheduled = false;
            bool revealed = false;
            long lastResizeTicks = Stopwatch.GetTimestamp();
            long shownTicks = 0;

            EventHandler idleHandler = null;
            System.Windows.Forms.Timer fallbackTimer = null;

            Action<string> reveal = delegate(string reason)
            {
                if (revealed || form.IsDisposed) return;

                revealed = true;
                revealScheduled = false;

                if (idleHandler != null)
                    Application.Idle -= idleHandler;

                if (fallbackTimer != null)
                {
                    fallbackTimer.Stop();
                    fallbackTimer.Dispose();
                    fallbackTimer = null;
                }

                try
                {
                    SQ4KOUUIDiagnostics.Mark(
                        "STARTUP",
                        "PRESENTATION_REVEAL",
                        "reason=" + reason +
                        ";state=" + form.WindowState +
                        ";size=" + form.Width + "x" + form.Height +
                        ";display=" + display.Width + "x" + display.Height);

                    // All expensive startup/layout/first-display work has already
                    // happened while the form was fully transparent.
                    form.Opacity = 1.0;
                    form.Activate();
                    form.Update();

                    if (afterReveal != null)
                    {
                        SQ4KOUUIDiagnostics.Mark("STARTUP", "AFTER_REVEAL_SCHEDULED", null);
                        form.BeginInvoke(new MethodInvoker(delegate
                        {
                            SQ4KOUUIDiagnostics.Mark("STARTUP", "AFTER_REVEAL_BEGIN", null);
                            try
                            {
                                afterReveal();
                            }
                            finally
                            {
                                SQ4KOUUIDiagnostics.Mark("STARTUP", "AFTER_REVEAL_END", null);
                            }
                        }));
                    }
                }
                catch
                {
                    // Presentation gating must never prevent startup.
                    try { form.Opacity = 1.0; } catch { }
                }
            };

            idleHandler = delegate
            {
                if (revealed || !shown || !firstDisplayPaintComplete) return;

                double msSinceResize =
                    (double)(Stopwatch.GetTimestamp() - Interlocked.Read(ref lastResizeTicks)) *
                    1000.0 / Stopwatch.Frequency;

                // Do not expose the form while maximize/restore/layout is still
                // producing resize traffic. The next Idle turn will retry.
                if (msSinceResize < 150.0) return;

                reveal("first_paint_plus_idle");
            };

            form.Resize += delegate
            {
                Interlocked.Exchange(ref lastResizeTicks, Stopwatch.GetTimestamp());
            };

            display.Paint += delegate
            {
                if (!shown || revealed) return;

                firstDisplayPaintComplete = true;
                if (!revealScheduled)
                {
                    revealScheduled = true;
                    Application.Idle += idleHandler;
                }
            };

            form.Shown += delegate
            {
                shown = true;
                shownTicks = Stopwatch.GetTimestamp();
                Interlocked.Exchange(ref lastResizeTicks, shownTicks);

                SQ4KOUUIDiagnostics.Mark(
                    "STARTUP",
                    "PRESENTATION_GATE_SHOWN",
                    "opacity=" + form.Opacity +
                    ";state=" + form.WindowState +
                    ";size=" + form.Width + "x" + form.Height);

                // Ensure a paint is requested even with AutoStart disabled.
                try { display.Invalidate(); } catch { }

                // Safety fallback only: if no display Paint occurs, expose a
                // stable form after 2.5 s rather than leave an invisible app.
                fallbackTimer = new System.Windows.Forms.Timer();
                fallbackTimer.Interval = 100;
                fallbackTimer.Tick += delegate
                {
                    if (revealed || form.IsDisposed) return;

                    double shownMs =
                        (double)(Stopwatch.GetTimestamp() - shownTicks) *
                        1000.0 / Stopwatch.Frequency;
                    double resizeMs =
                        (double)(Stopwatch.GetTimestamp() - Interlocked.Read(ref lastResizeTicks)) *
                        1000.0 / Stopwatch.Frequency;

                    if (shownMs >= 2500.0 && resizeMs >= 250.0)
                        reveal(firstDisplayPaintComplete ? "fallback_after_paint" : "fallback_no_display_paint");
                };
                fallbackTimer.Start();
            };

            // Set before the WinForms message loop shows the main form.
            // The window exists and can layout/paint, but intermediate frames
            // are not visible to the operator.
            form.Opacity = 0.0;

            SQ4KOUUIDiagnostics.Mark(
                "STARTUP",
                "PRESENTATION_GATE_ARMED",
                "opacity=0;policy=first_display_paint_then_idle");
        }
    }
}
