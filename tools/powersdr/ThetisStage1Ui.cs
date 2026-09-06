// PowerSDR -> Thetis UI Stage 2 - SQ4KOU
// Native KE9NS PowerSDR FLEX-5000 radio/DSP backend remains unchanged.
// This file only rearranges existing PowerSDR WinForms controls.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PowerSDR
{
    internal static class ThetisStage1Ui
    {
        private const string RootName = "sq4kouThetisStage2Root";
        private static readonly Color Bg = Color.FromArgb(18, 20, 24);
        private static readonly Color PanelBg = Color.FromArgb(29, 32, 37);
        private static readonly Color PanelBg2 = Color.FromArgb(37, 41, 47);
        private static readonly Color Fg = Color.FromArgb(226, 230, 235);
        private static readonly Color Muted = Color.FromArgb(137, 148, 160);
        private static readonly Color Accent = Color.FromArgb(76, 174, 255);
        private static readonly Color Tx = Color.FromArgb(238, 92, 92);

        private sealed class UiState
        {
            public Form Form;
            public Label Status;
            public Panel BandHost;
            public Panel ModeSpecificHost;
            public Panel RX2BandHost;
            public Timer Timer;
        }

        private static readonly Dictionary<Form, UiState> States = new Dictionary<Form, UiState>();

        public static void Install(Form form)
        {
            if (form == null) return;
            form.Shown += delegate
            {
                try { form.BeginInvoke((MethodInvoker)delegate { Apply(form); }); }
                catch { }
            };
        }

        private static void Apply(Form form)
        {
            if (form == null || form.IsDisposed) return;
            if (form.Controls.Find(RootName, true).Length != 0) return;

            form.SuspendLayout();
            try
            {
                form.BackColor = Bg;
                form.ForeColor = Fg;
                form.MinimumSize = new Size(Math.Max(1180, form.MinimumSize.Width), Math.Max(720, form.MinimumSize.Height));

                Control menu = Find(form, "menuStrip1");
                if (menu != null) menu.Dock = DockStyle.Top;

                Panel root = new Panel();
                root.Name = RootName;
                root.Dock = DockStyle.Fill;
                root.BackColor = Bg;
                root.Padding = new Padding(4);

                Panel header = new Panel();
                header.Name = "sq4kouThetisHeader";
                header.Dock = DockStyle.Top;
                header.Height = 172;
                header.BackColor = Bg;
                header.Padding = new Padding(0, 0, 0, 4);

                TableLayoutPanel vfoRow = new TableLayoutPanel();
                vfoRow.Dock = DockStyle.Top;
                vfoRow.Height = 88;
                vfoRow.ColumnCount = 4;
                vfoRow.RowCount = 1;
                vfoRow.Margin = new Padding(0);
                vfoRow.Padding = new Padding(0);
                vfoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
                vfoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
                vfoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
                vfoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
                vfoRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                vfoRow.BackColor = Bg;

                Control vfoA = Find(form, "grpVFOA");
                Control vfoTools = Find(form, "grpVFOBetween");
                Control vfoB = Find(form, "grpVFOB");
                PrepareDockPanel(vfoA);
                PrepareDockPanel(vfoTools);
                PrepareDockPanel(vfoB);
                if (vfoA != null) vfoRow.Controls.Add(vfoA, 0, 0);
                if (vfoTools != null) vfoRow.Controls.Add(vfoTools, 1, 0);
                if (vfoB != null) vfoRow.Controls.Add(vfoB, 2, 0);

                Panel quick = MakeQuickPanel(form);
                Label status = (Label)quick.Controls[0];
                vfoRow.Controls.Add(quick, 3, 0);

                TableLayoutPanel commandRow = new TableLayoutPanel();
                commandRow.Dock = DockStyle.Fill;
                commandRow.ColumnCount = 4;
                commandRow.RowCount = 1;
                commandRow.Margin = new Padding(0);
                commandRow.Padding = new Padding(0);
                commandRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
                commandRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23F));
                commandRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
                commandRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
                commandRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                commandRow.BackColor = Bg;

                Panel bandHost = MakeSection("BAND");
                Panel modeHost = MakeSection("MODE");
                Panel filterHost = MakeSection("FILTER");
                Panel dspHost = MakeSection("DSP");
                commandRow.Controls.Add(bandHost, 0, 0);
                commandRow.Controls.Add(modeHost, 1, 0);
                commandRow.Controls.Add(filterHost, 2, 0);
                commandRow.Controls.Add(dspHost, 3, 0);

                MoveOverlay(form, bandHost, "panelBandGN", "panelBandVHF", "panelBandHF");
                MoveSingle(form, modeHost, "panelMode");
                MoveSingle(form, filterHost, "panelFilter");
                MoveSingle(form, dspHost, "panelDSP");

                header.Controls.Add(commandRow);
                header.Controls.Add(vfoRow);

                TableLayoutPanel body = new TableLayoutPanel();
                body.Dock = DockStyle.Fill;
                body.ColumnCount = 2;
                body.RowCount = 1;
                body.Margin = new Padding(0);
                body.Padding = new Padding(0);
                body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
                body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                body.BackColor = Bg;

                Panel displayHost = new Panel();
                displayHost.Dock = DockStyle.Fill;
                displayHost.Margin = new Padding(0, 0, 4, 0);
                displayHost.Padding = new Padding(0);
                displayHost.BackColor = Color.Black;

                Control displayTools = Find(form, "panelDisplay2");
                Control display = Find(form, "panelDisplay");
                if (displayTools != null)
                {
                    displayTools.Parent = displayHost;
                    displayTools.Dock = DockStyle.Top;
                    displayTools.Height = Math.Max(34, Math.Min(52, displayTools.Height));
                    displayTools.Margin = new Padding(0);
                    displayTools.BackColor = PanelBg;
                }
                if (display != null)
                {
                    display.Parent = displayHost;
                    display.Dock = DockStyle.Fill;
                    display.Margin = new Padding(0);
                    display.BackColor = Color.Black;
                }

                TabControl sideTabs = MakeSideTabs(form);
                body.Controls.Add(displayHost, 0, 0);
                body.Controls.Add(sideTabs, 1, 0);

                root.Controls.Add(body);
                root.Controls.Add(header);
                form.Controls.Add(root);
                root.BringToFront();
                if (menu != null) menu.BringToFront();

                StyleTree(root);
                if (menu != null) StyleMenu(menu as MenuStrip);
                BringVisibleToFront(bandHost);

                UiState state = new UiState();
                state.Form = form;
                state.Status = status;
                state.BandHost = bandHost;
                state.ModeSpecificHost = FindHost(sideTabs, "sq4kouModeSpecificHost");
                state.RX2BandHost = FindHost(sideTabs, "sq4kouRX2BandHost");
                state.Timer = new Timer();
                state.Timer.Interval = 150;
                state.Timer.Tick += delegate { RefreshState(state); };
                state.Timer.Start();
                States[form] = state;

                form.FormClosed += delegate
                {
                    UiState s;
                    if (States.TryGetValue(form, out s))
                    {
                        try { s.Timer.Stop(); s.Timer.Dispose(); } catch { }
                        States.Remove(form);
                    }
                };

                RefreshState(state);
            }
            finally
            {
                form.ResumeLayout(true);
            }
        }

        private static Panel MakeQuickPanel(Form form)
        {
            Panel p = new Panel();
            p.Dock = DockStyle.Fill;
            p.Margin = new Padding(2);
            p.Padding = new Padding(5, 4, 5, 4);
            p.BackColor = PanelBg;

            Label status = new Label();
            status.Dock = DockStyle.Top;
            status.Height = 26;
            status.Text = "RX";
            status.TextAlign = ContentAlignment.MiddleCenter;
            status.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            status.ForeColor = Accent;
            status.BackColor = Color.Transparent;

            FlowLayoutPanel f = new FlowLayoutPanel();
            f.Dock = DockStyle.Fill;
            f.FlowDirection = FlowDirection.LeftToRight;
            f.WrapContents = true;
            f.AutoScroll = false;
            f.Padding = new Padding(0, 3, 0, 0);
            f.BackColor = Color.Transparent;

            string[] names = new string[]
            {
                "chkPower", "chkMOX", "chkTUN", "chkVFOSplit",
                "chkRIT", "chkXIT", "chkMUT", "chkFWCATU"
            };
            foreach (string name in names)
            {
                Control c = Find(form, name);
                if (c == null) continue;
                c.Parent = f;
                c.Dock = DockStyle.None;
                c.AutoSize = false;
                c.Width = Math.Max(48, Math.Min(78, c.Width));
                c.Height = 22;
                c.Margin = new Padding(1);
            }

            p.Controls.Add(f);
            p.Controls.Add(status);
            return p;
        }

        private static TabControl MakeSideTabs(Form form)
        {
            TabControl tabs = new TabControl();
            tabs.Name = "sq4kouThetisSideTabs";
            tabs.Dock = DockStyle.Fill;
            tabs.Margin = new Padding(0);
            tabs.Padding = new Point(10, 4);
            tabs.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            TabPage rx = MakeTab("RX");
            TabPage tx = MakeTab("TX");
            TabPage rx2 = MakeTab("RX2");
            TabPage misc = MakeTab("TOOLS");
            tabs.TabPages.Add(rx);
            tabs.TabPages.Add(tx);
            tabs.TabPages.Add(rx2);
            tabs.TabPages.Add(misc);

            Control meter = Find(form, "grpMultimeter");
            Control sound = Find(form, "panelSoundControls");
            Control antenna = Find(form, "panelAntenna");
            Control options = Find(form, "panelOptions");
            StackTop(rx, meter, 115);
            StackTop(rx, sound, 185);
            StackTop(rx, antenna, 92);
            FillLast(rx, options);

            Panel modeSpecific = new Panel();
            modeSpecific.Name = "sq4kouModeSpecificHost";
            modeSpecific.Dock = DockStyle.Fill;
            modeSpecific.BackColor = PanelBg;
            tx.Controls.Add(modeSpecific);
            MoveOverlay(form, modeSpecific,
                "panelModeSpecificPhone", "panelModeSpecificFM",
                "panelModeSpecificCW", "panelModeSpecificDigital");

            Panel rx2Band = new Panel();
            rx2Band.Name = "sq4kouRX2BandHost";
            rx2Band.Dock = DockStyle.Top;
            rx2Band.Height = 102;
            rx2Band.BackColor = PanelBg;
            rx2.Controls.Add(rx2Band);
            MoveOverlay(form, rx2Band, "panelBandGNRX2", "panelBandVHFRX2", "panelBandHFRX2");
            StackTop(rx2, Find(form, "grpRX2Meter"), 112);
            StackTop(rx2, Find(form, "panelRX2Mode"), 92);
            StackTop(rx2, Find(form, "panelRX2Filter"), 155);
            StackTop(rx2, Find(form, "panelRX2DSP"), 92);
            FillLast(rx2, Find(form, "panelRX2Mixer"));

            StackTop(misc, Find(form, "panelMultiRX"), 120);
            StackTop(misc, Find(form, "grpDisplaySplit"), 110);
            StackTop(misc, Find(form, "panelDateTime"), 90);
            FillLast(misc, Find(form, "panelTSRadar"));

            BringVisibleToFront(modeSpecific);
            BringVisibleToFront(rx2Band);
            return tabs;
        }

        private static TabPage MakeTab(string text)
        {
            TabPage t = new TabPage(text);
            t.BackColor = PanelBg;
            t.ForeColor = Fg;
            t.Padding = new Padding(4);
            return t;
        }

        private static Panel MakeSection(string caption)
        {
            Panel outer = new Panel();
            outer.Dock = DockStyle.Fill;
            outer.Margin = new Padding(2);
            outer.Padding = new Padding(3, 17, 3, 3);
            outer.BackColor = PanelBg;

            Label l = new Label();
            l.Dock = DockStyle.Top;
            l.Height = 15;
            l.Location = new Point(3, 1);
            l.Text = caption;
            l.TextAlign = ContentAlignment.MiddleLeft;
            l.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            l.ForeColor = Muted;
            l.BackColor = Color.Transparent;
            outer.Controls.Add(l);
            l.BringToFront();
            return outer;
        }

        private static void MoveSingle(Form form, Panel host, string name)
        {
            Control c = Find(form, name);
            if (c == null) return;
            c.Parent = host;
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(0);
            c.BackColor = Color.Transparent;
            c.BringToFront();
        }

        private static void MoveOverlay(Form form, Panel host, params string[] names)
        {
            foreach (string name in names)
            {
                Control c = Find(form, name);
                if (c == null) continue;
                c.Parent = host;
                c.Dock = DockStyle.Fill;
                c.Margin = new Padding(0);
                c.BackColor = Color.Transparent;
            }
            BringVisibleToFront(host);
        }

        private static void StackTop(Control host, Control c, int height)
        {
            if (host == null || c == null) return;
            c.Parent = host;
            c.Dock = DockStyle.Top;
            c.Height = Math.Max(height, c.Height);
            c.Margin = new Padding(0, 0, 0, 3);
            c.BackColor = Color.Transparent;
            c.BringToFront();
        }

        private static void FillLast(Control host, Control c)
        {
            if (host == null || c == null) return;
            c.Parent = host;
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(0);
            c.BackColor = Color.Transparent;
            c.SendToBack();
        }

        private static void PrepareDockPanel(Control c)
        {
            if (c == null) return;
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(2);
            c.BackColor = PanelBg;
        }

        private static Control Find(Form form, string name)
        {
            if (form == null || String.IsNullOrEmpty(name)) return null;
            Control[] a = form.Controls.Find(name, true);
            return a == null || a.Length == 0 ? null : a[0];
        }

        private static Panel FindHost(Control root, string name)
        {
            if (root == null) return null;
            Control[] a = root.Controls.Find(name, true);
            if (a == null || a.Length == 0) return null;
            return a[0] as Panel;
        }

        private static void BringVisibleToFront(Control host)
        {
            if (host == null) return;
            foreach (Control c in host.Controls)
            {
                if (c is Label) continue;
                if (c.Visible)
                {
                    c.BringToFront();
                    return;
                }
            }
        }

        private static void RefreshState(UiState state)
        {
            if (state == null || state.Form == null || state.Form.IsDisposed) return;
            BringVisibleToFront(state.BandHost);
            BringVisibleToFront(state.ModeSpecificHost);
            BringVisibleToFront(state.RX2BandHost);

            bool tx = ReadChecked(state.Form, "chkMOX") || ReadChecked(state.Form, "chkTUN");
            bool power = ReadChecked(state.Form, "chkPower");
            if (state.Status != null)
            {
                state.Status.Text = tx ? "TX" : (power ? "RX" : "OFF");
                state.Status.ForeColor = tx ? Tx : (power ? Accent : Muted);
            }
        }

        private static bool ReadChecked(Form form, string name)
        {
            Control c = Find(form, name);
            CheckBox cb = c as CheckBox;
            if (cb != null) return cb.Checked;
            RadioButton rb = c as RadioButton;
            if (rb != null) return rb.Checked;
            return false;
        }

        private static void StyleTree(Control root)
        {
            if (root == null) return;
            foreach (Control c in root.Controls)
            {
                if (c is MenuStrip || c is StatusStrip || c is ToolStrip) { }
                else if (c is TabPage)
                {
                    c.BackColor = PanelBg;
                    c.ForeColor = Fg;
                }
                else if (c is TextBoxBase)
                {
                    c.BackColor = Color.FromArgb(12, 14, 17);
                    c.ForeColor = Fg;
                }
                else if (c is ComboBox)
                {
                    c.BackColor = PanelBg2;
                    c.ForeColor = Fg;
                }
                else if (c is ButtonBase)
                {
                    ButtonBase b = (ButtonBase)c;
                    b.ForeColor = Fg;
                    b.BackColor = PanelBg2;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.BorderColor = Color.FromArgb(67, 73, 82);
                }
                else if (c is Label)
                {
                    c.ForeColor = c.ForeColor == Color.Yellow ? Color.Yellow : Fg;
                    if (c.BackColor != Color.Transparent) c.BackColor = Color.Transparent;
                }
                else if (c is Panel || c is GroupBox)
                {
                    if (c.BackColor != Color.Black) c.BackColor = PanelBg;
                    c.ForeColor = Fg;
                }
                if (c.HasChildren) StyleTree(c);
            }
        }

        private static void StyleMenu(MenuStrip menu)
        {
            if (menu == null) return;
            menu.BackColor = Color.FromArgb(10, 11, 13);
            menu.ForeColor = Fg;
            foreach (ToolStripItem item in menu.Items) item.ForeColor = Fg;
        }
    }
}
