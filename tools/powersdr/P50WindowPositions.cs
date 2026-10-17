// PowerSDR FLEX-5000 P50: Shift+left-drag of existing console panels.
// Independent of the Thetis meter containers and the native RF/DSP/display paths.
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
        private sealed class P50MoveGroup
        {
            internal readonly string Name;
            internal readonly List<Control> Controls = new List<Control>();
            internal Point Offset;

            internal P50MoveGroup(string name, Point offset)
            {
                Name = name;
                Offset = offset;
            }
        }

        private sealed class P50MouseFilter : IMessageFilter
        {
            private readonly Console owner;
            internal P50MouseFilter(Console console) { owner = console; }
            public bool PreFilterMessage(ref Message m)
            {
                return owner.P50FilterMouse(m);
            }
        }

        private const int P50WmMouseMove = 0x0200;
        private const int P50WmLeftDown = 0x0201;
        private const int P50WmLeftUp = 0x0202;
        private const int P50WmLeftDoubleClick = 0x0203;
        private const string P50DatabaseTable = "SQ4KOU_WindowPositions_P50";

        private readonly Dictionary<Control, P50MoveGroup> p50PanelGroups =
            new Dictionary<Control, P50MoveGroup>();
        private readonly List<P50MoveGroup> p50Groups = new List<P50MoveGroup>();
        private readonly Dictionary<Control, Point> p50DragOrigins =
            new Dictionary<Control, Point>();

        private P50MouseFilter p50Filter;
        private P50MoveGroup p50ActiveGroup;
        private Control p50ActiveControl;
        private Point p50DragMouseStart;
        private Point p50DragApplied;
        private bool p50Applying;

        internal void P50InitWindowPositions()
        {
            if (p50Filter != null) return;

            Dictionary<string, Point> saved = P50ReadOffsets();

            // These are native, top-level WinForms panels, verified in
            // console.Designer.cs. Alternative band/mode panels move together.
            P50AddGroup("RX1_Meters", saved, grpMultimeter, pwrMstWatts, pwrMstSWR);
            P50AddGroup("RX2_Meter", saved, grpRX2Meter);
            P50AddGroup("RX1_Bands", saved, panelBandHF, panelBandGN, panelBandVHF);
            P50AddGroup("RX2_Bands", saved, panelBandHFRX2, panelBandGNRX2, panelBandVHFRX2);
            P50AddGroup("RX1_Mode", saved, panelMode);
            P50AddGroup("RX2_Mode", saved, panelRX2Mode);
            P50AddGroup("RX1_Filter", saved, panelFilter);
            P50AddGroup("RX2_Filter", saved, panelRX2Filter);
            P50AddGroup("VFO_A", saved, grpVFOA, VFODialA, VFODialAA);
            P50AddGroup("VFO_B", saved, grpVFOB, VFODialB, VFODialBB);
            P50AddGroup("VFO_Between", saved, grpVFOBetween);
            P50AddGroup("RX1_DSP", saved, panelDSP);
            P50AddGroup("RX2_DSP", saved, panelRX2DSP);
            P50AddGroup("Options", saved, panelOptions);
            P50AddGroup("Antenna", saved, panelAntenna);
            P50AddGroup("Sound", saved, panelSoundControls);
            P50AddGroup("Mode_Specific", saved,
                panelModeSpecificPhone, panelModeSpecificCW,
                panelModeSpecificFM, panelModeSpecificDigital);
            P50AddGroup("Date_Time", saved, panelDateTime);

            // Do not register panelDisplay, panelDisplay2 or Meters/Gadgets.
            // Their native layout, spectrum drawing and P48 window state stay intact.
            foreach (P50MoveGroup group in p50Groups)
            {
                if (group.Offset == Point.Empty) continue;
                p50Applying = true;
                try
                {
                    foreach (Control panel in group.Controls)
                    {
                        panel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        panel.Location = new Point(
                            panel.Left + group.Offset.X, panel.Top + group.Offset.Y);
                    }
                }
                finally { p50Applying = false; }
            }

            foreach (Control panel in p50PanelGroups.Keys)
                panel.LocationChanged += P50NativePanelLocationChanged;

            p50Filter = new P50MouseFilter(this);
            Application.AddMessageFilter(p50Filter);
            Disposed += delegate(object sender, EventArgs e)
            {
                if (p50Filter != null)
                {
                    Application.RemoveMessageFilter(p50Filter);
                    p50Filter = null;
                }
            };

            System.Diagnostics.Debug.WriteLine(
                "P50 window positions enabled: " + p50Groups.Count.ToString() +
                " groups; Shift+left-drag only.");
        }

        private void P50AddGroup(
            string name, Dictionary<string, Point> saved, params Control[] members)
        {
            Point offset;
            if (!saved.TryGetValue(name, out offset)) offset = Point.Empty;
            P50MoveGroup group = new P50MoveGroup(name, offset);

            foreach (Control member in members)
            {
                // Never reposition docked controls or children of other panels.
                if (member == null || member.Parent != this ||
                    member.Dock != DockStyle.None)
                    continue;

                if (p50PanelGroups.ContainsKey(member))
                    throw new InvalidOperationException("P50 duplicate panel: " + member.Name);

                group.Controls.Add(member);
                p50PanelGroups.Add(member, group);
            }
            if (group.Controls.Count != 0) p50Groups.Add(group);
        }

        private Dictionary<string, Point> P50ReadOffsets()
        {
            Dictionary<string, Point> result = new Dictionary<string, Point>(
                StringComparer.Ordinal);
            ArrayList rows = DB.GetVars(P50DatabaseTable);
            if (rows == null) return result;

            foreach (object row in rows)
            {
                string s = row as string;
                if (String.IsNullOrEmpty(s)) continue;
                int slash = s.IndexOf('/');
                if (slash <= 0) continue;
                string[] xy = s.Substring(slash + 1).Split(',');
                if (xy.Length != 2) continue;

                int x, y;
                if (!Int32.TryParse(xy[0], NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out x) ||
                    !Int32.TryParse(xy[1], NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out y))
                    continue;

                // Prevent corrupt offsets from putting all panels off screen.
                if (Math.Abs((long)x) > 10000 || Math.Abs((long)y) > 10000)
                    continue;

                result[s.Substring(0, slash)] = new Point(x, y);
            }
            return result;
        }

        private void P50StoreOffsets()
        {
            ArrayList rows = new ArrayList();
            foreach (P50MoveGroup group in p50Groups)
            {
                rows.Add(group.Name + "/" +
                    group.Offset.X.ToString(CultureInfo.InvariantCulture) + "," +
                    group.Offset.Y.ToString(CultureInfo.InvariantCulture));
            }
            DB.ReplaceVars(P50DatabaseTable, ref rows);
            DB.Update();
        }

        private void P50NativePanelLocationChanged(object sender, EventArgs e)
        {
            // Existing KE9NS layout code can reapply native panel positions,
            // e.g. on a mode change. Preserve the user offset in that case.
            if (p50Applying || p50ActiveGroup != null) return;
            Control panel = sender as Control;
            P50MoveGroup group;
            if (panel == null || !p50PanelGroups.TryGetValue(panel, out group) ||
                group.Offset == Point.Empty || panel.Parent != this)
                return;

            p50Applying = true;
            try
            {
                panel.Location = new Point(
                    panel.Left + group.Offset.X, panel.Top + group.Offset.Y);
            }
            finally { p50Applying = false; }
        }

        private bool P50FilterMouse(Message message)
        {
            if (p50ActiveGroup != null)
            {
                if (message.Msg == P50WmMouseMove)
                {
                    P50DragToMouse();
                    return true;
                }
                if (message.Msg == P50WmLeftUp)
                {
                    P50FinishDrag();
                    return true;
                }
                return false;
            }

            if (message.Msg != P50WmLeftDown &&
                message.Msg != P50WmLeftDoubleClick)
                return false;

            if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
                return false;

            Control hit = Control.FromHandle(message.HWnd);
            if (hit == null) hit = Control.FromChildHandle(message.HWnd);
            while (hit != null)
            {
                P50MoveGroup group;
                if (p50PanelGroups.TryGetValue(hit, out group) && hit.Visible)
                {
                    p50ActiveGroup = group;
                    p50ActiveControl = hit;
                    p50DragMouseStart = Control.MousePosition;
                    p50DragApplied = Point.Empty;
                    p50DragOrigins.Clear();

                    foreach (Control panel in group.Controls)
                        p50DragOrigins.Add(panel, panel.Location);

                    // Capture keeps receiving mouse-up even beyond a panel edge.
                    Capture = true;
                    return true;
                }
                if (hit == this) break;
                hit = hit.Parent;
            }
            return false;
        }

        private void P50DragToMouse()
        {
            if (p50ActiveGroup == null || p50ActiveControl == null) return;

            Point cursor = Control.MousePosition;
            Point delta = new Point(
                cursor.X - p50DragMouseStart.X,
                cursor.Y - p50DragMouseStart.Y);
            Point original = p50DragOrigins[p50ActiveControl];

            // Keep a grab area inside the main form to avoid losing panels.
            int desiredX = original.X + delta.X;
            int desiredY = original.Y + delta.Y;
            int minX = 32 - p50ActiveControl.Width;
            int minY = 24 - p50ActiveControl.Height;
            int maxX = Math.Max(minX, ClientSize.Width - 32);
            int maxY = Math.Max(minY, ClientSize.Height - 24);
            desiredX = Math.Max(minX, Math.Min(maxX, desiredX));
            desiredY = Math.Max(minY, Math.Min(maxY, desiredY));
            delta = new Point(desiredX - original.X, desiredY - original.Y);

            if (delta == p50DragApplied) return;
            p50Applying = true;
            try
            {
                foreach (Control panel in p50ActiveGroup.Controls)
                {
                    panel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    Point start = p50DragOrigins[panel];
                    panel.Location = new Point(start.X + delta.X, start.Y + delta.Y);
                }
            }
            finally { p50Applying = false; }
            p50DragApplied = delta;
        }

        private void P50FinishDrag()
        {
            P50MoveGroup moved = p50ActiveGroup;
            Point delta = p50DragApplied;
            p50ActiveGroup = null;
            p50ActiveControl = null;
            p50DragOrigins.Clear();
            Capture = false;

            if (moved == null || delta == Point.Empty) return;
            moved.Offset = new Point(
                moved.Offset.X + delta.X, moved.Offset.Y + delta.Y);
            try
            {
                P50StoreOffsets();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("P50 position save failed: " + ex);
                MessageBox.Show(this,
                    "Window position could not be saved:\n" + ex.Message,
                    "PowerSDR P50", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
