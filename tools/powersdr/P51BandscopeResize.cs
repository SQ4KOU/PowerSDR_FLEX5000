// P51 – native PowerSDR bandscope/waterfall resizing; no DSP or display-engine fork.
// Uses the existing panelDisplay -> picDisplay -> picDisplay_Resize() path.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace PowerSDR
{
    sealed unsafe public partial class Console
    {
        private const string P51SizeTable = "SQ4KOU_BandscopeSize_P51";
        private const int P51EdgePixels = 14;
        private const int P51MinWidth = 340;
        private const int P51MinHeight = 160;

        private bool p51Ready;
        private bool p51ApplyingSize;
        private bool p51Resizing;
        private bool p51Left, p51Right, p51Top, p51Bottom;
        private Point p51MouseStart;
        private Rectangle p51BoundsStart;
        private Size p51ImageStart;
        private Size p51LastSize;
        private Size p51UserSizeOffset;
        private readonly Dictionary<Control, Rectangle> p51ChildStart =
            new Dictionary<Control, Rectangle>();

        internal void P51InitBandscopeResize()
        {
            if (p51Ready) return;
            p51UserSizeOffset = P51LoadSizeOffset();
            p51Ready = true;

            // The parent/picture sizes are changed together. The original
            // picDisplay_Resize callback reinitializes the native renderer.
            if (p51UserSizeOffset != Size.Empty)
            {
                P51Snapshot();
                P51SetBounds(new Rectangle(
                    panelDisplay.Location,
                    Math.Max(P51MinWidth, panelDisplay.Width + p51UserSizeOffset.Width),
                    Math.Max(P51MinHeight, panelDisplay.Height + p51UserSizeOffset.Height)));
            }

            panelDisplay.SizeChanged += P51NativeDisplaySizeChanged;
            picDisplay.SizeChanged += P51NativeDisplaySizeChanged;
            System.Diagnostics.Debug.WriteLine("P51 bandscope resize enabled.");
        }

        private Size P51LoadSizeOffset()
        {
            ArrayList vars = DB.GetVars(P51SizeTable);
            if (vars == null) return Size.Empty;
            foreach (object entry in vars)
            {
                string row = entry as string;
                if (row == null || !row.StartsWith("Display/", StringComparison.Ordinal))
                    continue;
                string[] parts = row.Substring(8).Split(',');
                int w, h;
                if (parts.Length != 2 ||
                    !Int32.TryParse(parts[0], NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out w) ||
                    !Int32.TryParse(parts[1], NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out h))
                    continue;
                if (Math.Abs((long)w) > 10000 || Math.Abs((long)h) > 10000)
                    continue;
                return new Size(w, h);
            }
            return Size.Empty;
        }

        private void P51SaveSizeOffset()
        {
            ArrayList rows = new ArrayList();
            rows.Add("Display/" +
                p51UserSizeOffset.Width.ToString(CultureInfo.InvariantCulture) + "," +
                p51UserSizeOffset.Height.ToString(CultureInfo.InvariantCulture));
            DB.ReplaceVars(P51SizeTable, ref rows);
            DB.Update();
        }

        private void P51Snapshot()
        {
            p51BoundsStart = panelDisplay.Bounds;
            p51ImageStart = picDisplay.Size;
            p51LastSize = panelDisplay.Size;
            p51ChildStart.Clear();
            foreach (Control child in panelDisplay.Controls)
                if (child != picDisplay)
                    p51ChildStart[child] = child.Bounds;
        }

        private void P51SetBounds(Rectangle bounds)
        {
            int dw = bounds.Width - p51BoundsStart.Width;
            int dh = bounds.Height - p51BoundsStart.Height;
            p51ApplyingSize = true;
            p50Applying = true;
            try
            {
                panelDisplay.SuspendLayout();
                try
                {
                    // Never resize the entire Console: only its spectrum panel.
                    panelDisplay.Bounds = bounds;
                    picDisplay.Size = new Size(
                        Math.Max(100, p51ImageStart.Width + dw),
                        Math.Max(100, p51ImageStart.Height + dh));

                    // Keep native pan/zoom controls adjacent to the resized
                    // waterfall instead of leaving them at their old positions.
                    foreach (KeyValuePair<Control, Rectangle> kv in p51ChildStart)
                    {
                        Control c = kv.Key;
                        Rectangle r = kv.Value;
                        bool bottom = p51BoundsStart.Height - r.Bottom <= 65;
                        bool right = p51BoundsStart.Width - r.Right <= 80;
                        int x = r.X + (right ? dw : 0);
                        int y = r.Y + (bottom ? dh : 0);
                        int w = r.Width;
                        int h = r.Height;

                        if (c == ptbDisplayPan || c == ptbDisplayPan2)
                        {
                            w = Math.Max(40, r.Width +
                                (ptbDisplayPan2.Visible ? dw / 2 : dw));
                            // RX2 pan controls occupy two horizontal halves.
                            if (c == ptbDisplayPan2 && ptbDisplayPan2.Visible)
                                x = r.X + dw / 2;
                        }
                        else if (c == panelTSBandStack)
                        {
                            x = r.X + dw;
                            h = Math.Max(40, r.Height + dh);
                        }

                        c.Bounds = new Rectangle(x, y, w, h);
                    }
                }
                finally { panelDisplay.ResumeLayout(false); }
                p51LastSize = panelDisplay.Size;
                picDisplay.Invalidate();
            }
            finally
            {
                p50Applying = false;
                p51ApplyingSize = false;
            }
        }

        private void P51NativeDisplaySizeChanged(object sender, EventArgs e)
        {
            if (!p51Ready || p51ApplyingSize || p51Resizing ||
                p51UserSizeOffset == Size.Empty)
                return;

            Control changed = sender as Control;
            if (changed == null) return;
            p51ApplyingSize = true;
            try
            {
                // KE9NS recalculates both sizes when its main form changes.
                // Reinstate the user's delta on each native assignment.
                changed.Size = new Size(
                    Math.Max(changed == panelDisplay ? P51MinWidth : 100,
                        changed.Width + p51UserSizeOffset.Width),
                    Math.Max(changed == panelDisplay ? P51MinHeight : 100,
                        changed.Height + p51UserSizeOffset.Height));
            }
            finally { p51ApplyingSize = false; }
        }

        // Invoked first by the already installed P50 mouse filter.
        private bool P51FilterBandscopeResize(Message m)
        {
            if (p51Resizing)
            {
                if (m.Msg == P50WmMouseMove)
                {
                    P51DragResize();
                    return true;
                }
                if (m.Msg == P50WmLeftUp)
                {
                    P51FinishResize();
                    return true;
                }
                return false;
            }

            if (!p51Ready || p50ActiveGroup != null ||
                (m.Msg != P50WmLeftDown && m.Msg != P50WmLeftDoubleClick) ||
                (Control.ModifierKeys & Keys.Shift) == 0)
                return false;

            Control hit = Control.FromHandle(m.HWnd);
            if (hit == null) hit = Control.FromChildHandle(m.HWnd);
            bool inside = false;
            while (hit != null)
            {
                if (hit == panelDisplay) { inside = true; break; }
                if (hit == this) break;
                hit = hit.Parent;
            }
            if (!inside || !panelDisplay.Visible) return false;

            Point p = panelDisplay.PointToClient(Control.MousePosition);
            if (p.X < 0 || p.Y < 0 ||
                p.X > panelDisplay.ClientSize.Width ||
                p.Y > panelDisplay.ClientSize.Height)
                return false;

            p51Left = p.X <= P51EdgePixels;
            p51Right = p.X >= panelDisplay.ClientSize.Width - P51EdgePixels;
            p51Top = p.Y <= P51EdgePixels;
            p51Bottom = p.Y >= panelDisplay.ClientSize.Height - P51EdgePixels;
            if (!p51Left && !p51Right && !p51Top && !p51Bottom)
                return false; // P50 handles moving from the interior.

            p51MouseStart = Control.MousePosition;
            P51Snapshot();
            p51Resizing = true;
            Capture = true;
            return true;
        }

        private void P51DragResize()
        {
            Point mouse = Control.MousePosition;
            int dx = mouse.X - p51MouseStart.X;
            int dy = mouse.Y - p51MouseStart.Y;
            int left = p51BoundsStart.Left + (p51Left ? dx : 0);
            int right = p51BoundsStart.Right + (p51Right ? dx : 0);
            int top = p51BoundsStart.Top + (p51Top ? dy : 0);
            int bottom = p51BoundsStart.Bottom + (p51Bottom ? dy : 0);

            if (right - left < P51MinWidth)
            {
                if (p51Left) left = right - P51MinWidth;
                else right = left + P51MinWidth;
            }
            if (bottom - top < P51MinHeight)
            {
                if (p51Top) top = bottom - P51MinHeight;
                else bottom = top + P51MinHeight;
            }
            // Leave a visible grab area inside the main PowerSDR window.
            left = Math.Max(24 - (right - left), Math.Min(ClientSize.Width - 24, left));
            top = Math.Max(24 - (bottom - top), Math.Min(ClientSize.Height - 24, top));
            Rectangle bounds = new Rectangle(left, top, right - left, bottom - top);
            if (bounds.Size == p51LastSize &&
                bounds.Location == panelDisplay.Location) return;

            P51SetBounds(bounds);
        }

        private void P51FinishResize()
        {
            Rectangle final = panelDisplay.Bounds;
            bool changed = final != p51BoundsStart;
            p51Resizing = false;
            Capture = false;
            p51ChildStart.Clear();
            if (!changed) return;

            p51UserSizeOffset = new Size(
                p51UserSizeOffset.Width + final.Width - p51BoundsStart.Width,
                p51UserSizeOffset.Height + final.Height - p51BoundsStart.Height);

            // Shift+resize from left/top also changes the position. Share the
            // same P50 offset table as Shift+move; do not create a second source.
            P50MoveGroup displayGroup;
            if (p50PanelGroups.TryGetValue(panelDisplay, out displayGroup))
            {
                displayGroup.Offset = new Point(
                    displayGroup.Offset.X + final.Left - p51BoundsStart.Left,
                    displayGroup.Offset.Y + final.Top - p51BoundsStart.Top);
            }

            try
            {
                P50StoreOffsets();
                P51SaveSizeOffset();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("P51 bandscope save failed: " + ex);
                MessageBox.Show(this, "Bandscope/waterfall geometry was not saved:\n" +
                    ex.Message, "PowerSDR P51", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
