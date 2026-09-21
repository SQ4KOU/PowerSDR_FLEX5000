using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PowerSDR
{
    // P27: direct standalone port of Thetis 2023-02-26 MultiMeters2 setup surface.
    // Deliberate target-only difference: RX2 is not exposed for this FLEX-5000 build.
    internal sealed class P27ThetisMetersConfigForm : Form
    {
        private readonly Console console;
        private readonly ToolTip toolTip1;

        private readonly GroupBoxTS groupBoxTS28;
        private readonly ComboBoxTS comboContainerSelect;
        private readonly ButtonTS btnAddRX1Container;
        private readonly ButtonTS btnContainerDelete;
        private readonly CheckBoxTS chkContainerHighlight;
        private readonly CheckBoxTS chkContainerBorder;
        private readonly LabelTS lblContainerBackground;
        private readonly ColorButton clrbtnContainerBackground;

        private readonly ListBox lstMetersAvailable;
        private readonly ListBox lstMetersInUse;
        private readonly ButtonTS btnAddMeterItem;
        private readonly ButtonTS btnRemoveMeterItem;
        private readonly ButtonTS btnMeterUp;
        private readonly ButtonTS btnMeterDown;
        private readonly ButtonTS btnMeterCopySettings;
        private readonly ButtonTS btnMeterPasteSettings;

        private readonly GroupBoxTS grpMeterItemSettings;
        private readonly NumericUpDownTS nudMeterItemUpdateRate;
        private readonly LabelTS labelTS167;
        private readonly NumericUpDownTS nudMeterItemAttackRate;
        private readonly LabelTS labelTS168;
        private readonly NumericUpDownTS nudMeterItemDecayRate;
        private readonly LabelTS labelTS169;

        private readonly LabelTS lblMMLow;
        private readonly LabelTS lblMMHigh;
        private readonly ColorButton clrbtnMeterItemLow;
        private readonly ColorButton clrbtnMeterItemHigh;
        private readonly LabelTS labelTS163;
        private readonly ColorButton clrbtnMeterItemIndiciator;
        private readonly LabelTS lblMMBackground;
        private readonly ColorButton clrbtnMeterItemHBackground;

        private readonly CheckBoxTS chkMeterItemFadeOnRx;
        private readonly CheckBoxTS chkMeterItemFadeOnTx;
        private readonly CheckBoxTS chkMeterItemShadow;
        private readonly CheckBoxTS chkMeterItemHistory;
        private readonly ColorButton clrbtnMeterItemHistory;
        private readonly TrackBarTS tbMeterItemHistoryAlpha;
        private readonly LabelTS labelTS166;
        private readonly NumericUpDownTS nudMeterItemHistoryDuration;

        private readonly CheckBoxTS chkMeterItemSegmented;
        private readonly ColorButton clrbtnMeterItemSegmentedColour;
        private readonly CheckBoxTS chkMeterItemTitle;
        private readonly ColorButton clrbtnMeterItemMeterTitle;
        private readonly CheckBoxTS chkMeterItemPeakValue;
        private readonly ColorButton clrbtnMeterItemPeakValueColour;
        private readonly CheckBoxTS chkMeterItemPeakHold;
        private readonly ColorButton clrbtnMeterItemPeakHold;

        private readonly LabelTS lblMMEyeSize;
        private readonly NumericUpDownTS nudMeterItemEyeScale;
        private readonly CheckBoxTS chkMeterItemSignalAverage;
        private readonly CheckBoxTS chkMeterItemDarkMode;

        private bool _ignoreMeterItemChangeEvents;
        private bool _loading;
        private bool _shutdown;

        private MeterManager.clsIGSettings _itemGroupSettings;
        private MeterType _itemGroupSettingsMeterType = MeterType.NONE;

        private sealed class clsContainerComboboxItem
        {
            public string Text { get; set; }
            public string ID { get; set; }
            public override string ToString() { return Text; }
        }

        private sealed class clsMeterTypeComboboxItem
        {
            private readonly MeterType _meterType;
            private int _order;

            internal clsMeterTypeComboboxItem(MeterType mt, int order)
            {
                _meterType = mt;
                _order = order;
            }

            internal MeterType MeterType { get { return _meterType; } }
            internal int Order { get { return _order; } set { _order = value; } }

            public override string ToString()
            {
                return MeterManager.MeterName(_meterType);
            }
        }

        internal P27ThetisMetersConfigForm(Console c)
        {
            if (c == null) throw new ArgumentNullException("c");
            console = c;
            toolTip1 = new ToolTip();

            Text = "PowerSDR - Meters/Gadgets";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = true;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(724, 410);

            groupBoxTS28 = new GroupBoxTS();
            groupBoxTS28.Location = new Point(13, 12);
            groupBoxTS28.Name = "groupBoxTS28";
            groupBoxTS28.Size = new Size(703, 385);
            groupBoxTS28.TabStop = false;
            Controls.Add(groupBoxTS28);

            comboContainerSelect = new ComboBoxTS();
            comboContainerSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            comboContainerSelect.FormattingEnabled = true;
            comboContainerSelect.Location = new Point(18, 21);
            comboContainerSelect.Name = "comboContainerSelect";
            comboContainerSelect.Size = new Size(193, 21);
            comboContainerSelect.SelectedIndexChanged += comboContainerSelect_SelectedIndexChanged;
            groupBoxTS28.Controls.Add(comboContainerSelect);

            btnAddRX1Container = new ButtonTS();
            btnAddRX1Container.Location = new Point(237, 15);
            btnAddRX1Container.Name = "btnAddRX1Container";
            btnAddRX1Container.Size = new Size(70, 56);
            btnAddRX1Container.Text = "Add TRX1 Container";
            toolTip1.SetToolTip(btnAddRX1Container, "Add a meter item container that uses RX1 readings");
            btnAddRX1Container.Click += btnAddRX1Container_Click;
            groupBoxTS28.Controls.Add(btnAddRX1Container);

            btnContainerDelete = new ButtonTS();
            btnContainerDelete.Location = new Point(18, 48);
            btnContainerDelete.Name = "btnContainerDelete";
            btnContainerDelete.Size = new Size(59, 32);
            btnContainerDelete.Text = "Remove";
            toolTip1.SetToolTip(btnContainerDelete, "Removes the selected container and all meter items contained within");
            btnContainerDelete.Click += btnContainerDelete_Click;
            groupBoxTS28.Controls.Add(btnContainerDelete);

            chkContainerHighlight = NewCheck("chkContainerHighlight", "Highlight", 102, 54);
            chkContainerHighlight.RightToLeft = RightToLeft.Yes;
            chkContainerHighlight.CheckedChanged += chkContainerHighlight_CheckedChanged;
            groupBoxTS28.Controls.Add(chkContainerHighlight);

            chkContainerBorder = NewCheck("chkContainerBorder", "Border", 112, 77);
            chkContainerBorder.RightToLeft = RightToLeft.Yes;
            toolTip1.SetToolTip(chkContainerBorder, "Container has a border");
            chkContainerBorder.CheckedChanged += chkContainerBorder_CheckedChanged;
            groupBoxTS28.Controls.Add(chkContainerBorder);

            lblContainerBackground = NewLabel("labelTS161", "Backg:", 182, 55, 41, 13, ContentAlignment.TopLeft);
            groupBoxTS28.Controls.Add(lblContainerBackground);

            clrbtnContainerBackground = NewColorButton("clrbtnContainerBackground", Color.Black, 181, 71);
            toolTip1.SetToolTip(clrbtnContainerBackground, "Container Background Colour");
            clrbtnContainerBackground.Changed += clrbtnContainerBackground_Changed;
            groupBoxTS28.Controls.Add(clrbtnContainerBackground);

            lstMetersAvailable = NewMeterList("lstMetersAvailable", 18, 116, 140, 251);
            lstMetersAvailable.SelectedIndexChanged += lstMetersAvailable_SelectedIndexChanged;
            lstMetersAvailable.DoubleClick += lstMetersAvailable_DoubleClick;
            groupBoxTS28.Controls.Add(lstMetersAvailable);

            lstMetersInUse = NewMeterList("lstMetersInUse", 202, 116, 140, 251);
            lstMetersInUse.SelectedIndexChanged += lstMetersInUse_SelectedIndexChanged;
            lstMetersInUse.DoubleClick += lstMetersInUse_DoubleClick;
            groupBoxTS28.Controls.Add(lstMetersInUse);

            btnAddMeterItem = NewImageButton("btnAddMeterItem", P25MeterResources.arrow_right_black, ">", 164, 115);
            btnAddMeterItem.Click += btnAddMeterItem_Click;
            groupBoxTS28.Controls.Add(btnAddMeterItem);

            btnRemoveMeterItem = NewImageButton("btnRemoveMeterItem", P25MeterResources.arrow_left_black, "<", 164, 162);
            btnRemoveMeterItem.Click += btnRemoveMeterItem_Click;
            groupBoxTS28.Controls.Add(btnRemoveMeterItem);

            btnMeterUp = NewImageButton("btnMeterUp", P25MeterResources.arrow_up_black, "↑", 348, 115);
            btnMeterUp.Click += btnMeterUp_Click;
            groupBoxTS28.Controls.Add(btnMeterUp);

            btnMeterDown = NewImageButton("btnMeterDown", P25MeterResources.down_black, "↓", 348, 162);
            btnMeterDown.Click += btnMeterDown_Click;
            groupBoxTS28.Controls.Add(btnMeterDown);

            btnMeterCopySettings = NewImageButton("btnMeterCopySettings", P25MeterResources.pipette32, "C", 336, 293);
            toolTip1.SetToolTip(btnMeterCopySettings, "Copy settings and colours");
            btnMeterCopySettings.Click += btnMeterCopySettings_Click;
            groupBoxTS28.Controls.Add(btnMeterCopySettings);

            btnMeterPasteSettings = NewImageButton("btnMeterPasteSettings", P25MeterResources.brush32, "P", 336, 340);
            toolTip1.SetToolTip(btnMeterPasteSettings, "Paste settings and colours into suitable meter item");
            btnMeterPasteSettings.Click += btnMeterPasteSettings_Click;
            groupBoxTS28.Controls.Add(btnMeterPasteSettings);

            grpMeterItemSettings = new GroupBoxTS();
            grpMeterItemSettings.Location = new Point(403, 15);
            grpMeterItemSettings.Name = "grpMeterItemSettings";
            grpMeterItemSettings.Size = new Size(294, 352);
            grpMeterItemSettings.TabStop = false;
            grpMeterItemSettings.Text = "Settings";
            groupBoxTS28.Controls.Add(grpMeterItemSettings);

            labelTS167 = NewLabel("labelTS167", "Update (ms):", 22, 33, 79, 16, ContentAlignment.TopRight);
            grpMeterItemSettings.Controls.Add(labelTS167);
            nudMeterItemUpdateRate = NewNumeric("nudMeterItemUpdateRate", 107, 31, 56, 20, 50M, 5000M, 100M, 0, 1M);
            nudMeterItemUpdateRate.ValueChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(nudMeterItemUpdateRate);

            labelTS168 = NewLabel("labelTS168", "Attack:", 22, 58, 79, 16, ContentAlignment.TopRight);
            grpMeterItemSettings.Controls.Add(labelTS168);
            nudMeterItemAttackRate = NewNumeric("nudMeterItemAttackRate", 107, 56, 56, 20, 0M, 1M, 1M, 2, 0.05M);
            toolTip1.SetToolTip(nudMeterItemAttackRate, "The 'speed of rise' to a new value if above current");
            nudMeterItemAttackRate.ValueChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(nudMeterItemAttackRate);

            labelTS169 = NewLabel("labelTS169", "Decay:", 169, 58, 50, 16, ContentAlignment.TopRight);
            grpMeterItemSettings.Controls.Add(labelTS169);
            nudMeterItemDecayRate = NewNumeric("nudMeterItemDecayRate", 225, 56, 56, 20, 0M, 1M, 1M, 2, 0.05M);
            toolTip1.SetToolTip(nudMeterItemDecayRate, "The 'speed of fall' to the new value if below current");
            nudMeterItemDecayRate.ValueChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(nudMeterItemDecayRate);

            lblMMLow = NewLabel("lblMMLow", "Low:", 33, 91, 30, 13, ContentAlignment.TopLeft);
            grpMeterItemSettings.Controls.Add(lblMMLow);
            clrbtnMeterItemLow = NewColorButton("clrbtnMeterItemLow", Color.White, 61, 86);
            clrbtnMeterItemLow.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemLow);

            lblMMHigh = NewLabel("lblMMHigh", "High:", 107, 91, 32, 13, ContentAlignment.TopLeft);
            grpMeterItemSettings.Controls.Add(lblMMHigh);
            clrbtnMeterItemHigh = NewColorButton("clrbtnMeterItemHigh", Color.Red, 142, 86);
            clrbtnMeterItemHigh.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemHigh);

            labelTS163 = NewLabel("labelTS163", "Indicator:", 10, 120, 51, 13, ContentAlignment.TopLeft);
            grpMeterItemSettings.Controls.Add(labelTS163);
            clrbtnMeterItemIndiciator = NewColorButton("clrbtnMeterItemIndiciator", Color.Yellow, 61, 115);
            clrbtnMeterItemIndiciator.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemIndiciator);

            lblMMBackground = NewLabel("lblMMBackground", "Backg:", 20, 149, 41, 13, ContentAlignment.TopLeft);
            grpMeterItemSettings.Controls.Add(lblMMBackground);
            clrbtnMeterItemHBackground = NewColorButton("clrbtnMeterItemHBackground", Color.LimeGreen, 61, 144);
            toolTip1.SetToolTip(clrbtnMeterItemHBackground, "The fill colour of the signal history on the multimeter");
            clrbtnMeterItemHBackground.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemHBackground);

            labelTS166 = NewLabel("labelTS166", "History (ms):", 122, 146, 79, 16, ContentAlignment.TopRight);
            grpMeterItemSettings.Controls.Add(labelTS166);
            nudMeterItemHistoryDuration = NewNumeric("nudMeterItemHistoryDuration", 207, 144, 56, 20, 50M, 10000M, 2000M, 0, 1M);
            nudMeterItemHistoryDuration.ValueChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(nudMeterItemHistoryDuration);

            chkMeterItemFadeOnRx = NewCheck("chkMeterItemFadeOnRx", "Fade on RX", 18, 182);
            chkMeterItemFadeOnRx.CheckedChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemFadeOnRx);

            chkMeterItemFadeOnTx = NewCheck("chkMeterItemFadeOnTx", "Fade on TX", 18, 205);
            chkMeterItemFadeOnTx.CheckedChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemFadeOnTx);

            chkMeterItemShadow = NewCheck("chkMeterItemShadow", "Shadow", 164, 181);
            chkMeterItemShadow.CheckedChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemShadow);

            chkMeterItemHistory = NewCheck("chkMeterItemHistory", "Show History", 164, 204);
            chkMeterItemHistory.CheckedChanged += chkMeterItemHistory_CheckedChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemHistory);

            clrbtnMeterItemHistory = NewColorButton("clrbtnMeterItemHistory", Color.Yellow, 179, 222);
            clrbtnMeterItemHistory.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemHistory);

            tbMeterItemHistoryAlpha = new TrackBarTS();
            tbMeterItemHistoryAlpha.AutoSize = false;
            tbMeterItemHistoryAlpha.Location = new Point(221, 227);
            tbMeterItemHistoryAlpha.Maximum = 255;
            tbMeterItemHistoryAlpha.Name = "tbMeterItemHistoryAlpha";
            tbMeterItemHistoryAlpha.Size = new Size(66, 18);
            tbMeterItemHistoryAlpha.TickFrequency = 64;
            tbMeterItemHistoryAlpha.Value = 255;
            tbMeterItemHistoryAlpha.Scroll += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(tbMeterItemHistoryAlpha);

            chkMeterItemSegmented = NewCheck("chkMeterItemSegmented", "Segmented", 18, 228);
            chkMeterItemSegmented.CheckedChanged += chkMeterItemSegmented_CheckedChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemSegmented);

            clrbtnMeterItemSegmentedColour = NewColorButton("clrbtnMeterItemSegmentedColour", Color.Yellow, 99, 224);
            clrbtnMeterItemSegmentedColour.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemSegmentedColour);

            chkMeterItemTitle = NewCheck("chkMeterItemTitle", "Meter Title", 18, 254);
            chkMeterItemTitle.CheckedChanged += chkMeterItemTitle_CheckedChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemTitle);

            clrbtnMeterItemMeterTitle = NewColorButton("clrbtnMeterItemMeterTitle", Color.Yellow, 99, 251);
            clrbtnMeterItemMeterTitle.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemMeterTitle);

            chkMeterItemPeakValue = NewCheck("chkMeterItemPeakValue", "Peak Value", 17, 280);
            chkMeterItemPeakValue.CheckedChanged += chkMeterItemPeakValue_CheckedChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemPeakValue);

            clrbtnMeterItemPeakValueColour = NewColorButton("clrbtnMeterItemPeakValueColour", Color.Yellow, 99, 278);
            clrbtnMeterItemPeakValueColour.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemPeakValueColour);

            chkMeterItemPeakHold = NewCheck("chkMeterItemPeakHold", "Show Peak Hold", 164, 251);
            chkMeterItemPeakHold.CheckedChanged += chkMeterItemPeakHold_CheckedChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemPeakHold);

            clrbtnMeterItemPeakHold = NewColorButton("clrbtnMeterItemPeakHold", Color.Yellow, 179, 272);
            clrbtnMeterItemPeakHold.Changed += anyMeterColorChanged;
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemPeakHold);

            lblMMEyeSize = NewLabel("lblMMEyeSize", "Eye Size:", 13, 309, 64, 16, ContentAlignment.TopRight);
            grpMeterItemSettings.Controls.Add(lblMMEyeSize);
            nudMeterItemEyeScale = NewNumeric("nudMeterItemEyeScale", 83, 307, 56, 20, 0.01M, 1M, 1M, 2, 0.01M);
            toolTip1.SetToolTip(nudMeterItemEyeScale, "Size of the eye, 1.0 is full width of container");
            nudMeterItemEyeScale.ValueChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(nudMeterItemEyeScale);

            chkMeterItemSignalAverage = NewCheck("chkMeterItemSignalAverage", "Signal Average", 164, 302);
            toolTip1.SetToolTip(chkMeterItemSignalAverage, "Use sig average instead of sig");
            chkMeterItemSignalAverage.CheckedChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemSignalAverage);

            chkMeterItemDarkMode = NewCheck("chkMeterItemDarkMode", "Dark Mode", 164, 325);
            chkMeterItemDarkMode.CheckedChanged += anyMeterSettingChanged;
            grpMeterItemSettings.Controls.Add(chkMeterItemDarkMode);

            FormClosing += P27ThetisMetersConfigForm_FormClosing;
            VisibleChanged += P27ThetisMetersConfigForm_VisibleChanged;
            Shown += delegate { updateMeter2Controls(""); };

            updateMeter2Controls("");
        }

        private static ButtonTS NewImageButton(string name, Image image, string fallbackText, int x, int y)
        {
            ButtonTS b = new ButtonTS();
            b.Location = new Point(x, y);
            b.Name = name;
            b.Size = new Size(32, 32);
            if (image != null)
            {
                b.BackgroundImage = image;
                b.BackgroundImageLayout = ImageLayout.Center;
                b.Text = "";
            }
            else b.Text = fallbackText;
            b.UseVisualStyleBackColor = true;
            return b;
        }

        private static CheckBoxTS NewCheck(string name, string text, int x, int y)
        {
            CheckBoxTS c = new CheckBoxTS();
            c.AutoSize = true;
            c.Location = new Point(x, y);
            c.Name = name;
            c.Text = text;
            c.UseVisualStyleBackColor = true;
            return c;
        }

        private static LabelTS NewLabel(string name, string text, int x, int y, int w, int h, ContentAlignment align)
        {
            LabelTS l = new LabelTS();
            l.Location = new Point(x, y);
            l.Name = name;
            l.Size = new Size(w, h);
            l.Text = text;
            l.TextAlign = align;
            return l;
        }

        private static NumericUpDownTS NewNumeric(string name, int x, int y, int w, int h,
            decimal min, decimal max, decimal value, int decimalPlaces, decimal increment)
        {
            NumericUpDownTS n = new NumericUpDownTS();
            n.Location = new Point(x, y);
            n.Name = name;
            n.Size = new Size(w, h);
            n.Minimum = min;
            n.Maximum = max;
            n.DecimalPlaces = decimalPlaces;
            n.Increment = increment;
            n.Value = value;
            return n;
        }

        private static ColorButton NewColorButton(string name, Color color, int x, int y)
        {
            ColorButton b = new ColorButton();
            b.Automatic = "Automatic";
            b.Color = color;
            b.Location = new Point(x, y);
            b.MoreColors = "More Colors...";
            b.Name = name;
            b.Size = new Size(40, 23);
            return b;
        }

        private static ListBox NewMeterList(string name, int x, int y, int w, int h)
        {
            ListBox l = new ListBox();
            l.DrawMode = DrawMode.OwnerDrawFixed;
            l.FormattingEnabled = true;
            l.IntegralHeight = false;
            l.Location = new Point(x, y);
            l.Name = name;
            l.Size = new Size(w, h);
            l.DrawItem += lstMetersInUse_DrawItem;
            return l;
        }

        private void btnAddRX1Container_Click(object sender, EventArgs e)
        {
            if (MeterManager.TotalMeterContainers >= 10) return;
            string sId = MeterManager.AddMeterContainer(1, false, console.MOX);
            updateMeter2Controls(sId);
        }

        private void btnContainerDelete_Click(object sender, EventArgs e)
        {
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci == null) return;
            MeterManager.RemoveMeterContainer(cci.ID);
            updateMeter2Controls("");
        }

        private void updateMeter2Controls(string sId)
        {
            _loading = true;
            try
            {
                btnAddRX1Container.Enabled = MeterManager.TotalMeterContainers < 10;
                comboContainerSelect.Text = "";
                comboContainerSelect.Items.Clear();

                int i = 0;
                int nSelect = 0;
                foreach (KeyValuePair<string, ucMeter> kvp in MeterManager.MeterContainers)
                {
                    clsContainerComboboxItem cci = new clsContainerComboboxItem();
                    cci.Text = "Container " + (i + 1).ToString() + " TRX" + kvp.Value.RX.ToString();
                    cci.ID = kvp.Value.ID;
                    comboContainerSelect.Items.Add(cci);
                    if (cci.ID == sId) nSelect = i;
                    i++;
                }

                bool enabled = comboContainerSelect.Items.Count > 0;
                comboContainerSelect.Enabled = enabled;
                btnContainerDelete.Enabled = enabled;
                chkContainerHighlight.Enabled = enabled;
                chkContainerBorder.Enabled = enabled;
                clrbtnContainerBackground.Enabled = enabled;
                if (enabled) comboContainerSelect.SelectedIndex = Math.Max(0, Math.Min(nSelect, comboContainerSelect.Items.Count - 1));
            }
            finally { _loading = false; }

            comboContainerSelect_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private MeterManager.clsMeter meterFromSelectedContainer()
        {
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            return cci == null ? null : MeterManager.MeterFromId(cci.ID);
        }

        private void updateMeterLists()
        {
            MeterType keep = meterItemGroupTypefromSelected();
            lstMetersAvailable.Items.Clear();
            lstMetersInUse.Items.Clear();

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null)
            {
                grpMeterItemSettings.Enabled = false;
                return;
            }

            List<clsMeterTypeComboboxItem> inuse = new List<clsMeterTypeComboboxItem>();
            List<clsMeterTypeComboboxItem> notinuse = new List<clsMeterTypeComboboxItem>();
            for (int n = 1; n < (int)MeterType.LAST; n++)
            {
                MeterType mt = (MeterType)n;
                if (m.HasMeterType(mt)) inuse.Add(new clsMeterTypeComboboxItem(mt, m.GetOrderForMeterType(mt)));
                else notinuse.Add(new clsMeterTypeComboboxItem(mt, -1));
            }

            foreach (clsMeterTypeComboboxItem mtci in notinuse) lstMetersAvailable.Items.Add(mtci);

            int selected = -1;
            foreach (clsMeterTypeComboboxItem mtci in inuse.OrderBy(o => o.Order))
            {
                int idx = lstMetersInUse.Items.Add(mtci);
                if (mtci.MeterType == keep) selected = idx;
            }
            if (selected >= 0) lstMetersInUse.SelectedIndex = selected;

            lstMetersAvailable_SelectedIndexChanged(this, EventArgs.Empty);
            lstMetersInUse_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void comboContainerSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci == null) return;

            if (chkContainerHighlight.Checked) MeterManager.HighlightContainer(cci.ID);

            _loading = true;
            try
            {
                chkContainerBorder.Checked = MeterManager.ContainerHasBorder(cci.ID);
                clrbtnContainerBackground.Color = MeterManager.GetContainerBackgroundColour(cci.ID);
            }
            finally { _loading = false; }

            updateMeterLists();
        }

        private void chkContainerHighlight_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            MeterManager.HighlightContainer(chkContainerHighlight.Checked && cci != null ? cci.ID : "");
        }

        private void chkContainerBorder_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci != null) MeterManager.ContainerBorder(cci.ID, chkContainerBorder.Checked);
        }

        private void clrbtnContainerBackground_Changed(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci != null) MeterManager.ContainerBackgroundColour(cci.ID, clrbtnContainerBackground.Color);
        }

        private void btnAddMeterItem_Click(object sender, EventArgs e)
        {
            clsMeterTypeComboboxItem mti = lstMetersAvailable.SelectedItem as clsMeterTypeComboboxItem;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (mti == null || m == null) return;
            m.AddMeter(mti.MeterType);
            m.Rebuild();
            updateMeterLists();
            SelectMeterType(mti.MeterType);
        }

        private void btnRemoveMeterItem_Click(object sender, EventArgs e)
        {
            clsMeterTypeComboboxItem mti = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (mti == null || m == null) return;
            m.RemoveMeterType(mti.MeterType, true);
            updateMeterLists();
        }

        private void btnMeterUp_Click(object sender, EventArgs e)
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (m == null || mtci == null) return;
            int n = lstMetersInUse.SelectedIndex - 1;
            if (n < 0) return;
            m.SetOrderForMeterType(mtci.MeterType, n, true, true);
            updateMeterLists();
            SelectMeterType(mtci.MeterType);
        }

        private void btnMeterDown_Click(object sender, EventArgs e)
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (m == null || mtci == null) return;
            int n = lstMetersInUse.SelectedIndex + 1;
            if (n > lstMetersInUse.Items.Count - 1) return;
            m.SetOrderForMeterType(mtci.MeterType, n, true, false);
            updateMeterLists();
            SelectMeterType(mtci.MeterType);
        }

        private void SelectMeterType(MeterType mt)
        {
            for (int i = 0; i < lstMetersInUse.Items.Count; i++)
            {
                clsMeterTypeComboboxItem x = lstMetersInUse.Items[i] as clsMeterTypeComboboxItem;
                if (x != null && x.MeterType == mt)
                {
                    lstMetersInUse.SelectedIndex = i;
                    break;
                }
            }
        }

        private void lstMetersAvailable_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAddMeterItem.Enabled = lstMetersAvailable.SelectedIndex >= 0;
        }

        private void lstMetersInUse_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = lstMetersInUse.SelectedIndex >= 0;
            if (enabled) updateItemSettingsControlsForSelected();
            btnRemoveMeterItem.Enabled = enabled;
            btnMeterUp.Enabled = enabled;
            btnMeterDown.Enabled = enabled;
            btnMeterCopySettings.Enabled = enabled;
            btnMeterPasteSettings.Enabled = enabled && canPasteSettings();
            grpMeterItemSettings.Enabled = enabled;
        }

        private void lstMetersAvailable_DoubleClick(object sender, EventArgs e) { btnAddMeterItem_Click(sender, e); }
        private void lstMetersInUse_DoubleClick(object sender, EventArgs e) { btnRemoveMeterItem_Click(sender, e); }

        private static void lstMetersInUse_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            ListBox list = sender as ListBox;
            if (list == null || e.Index < 0 || e.Index >= list.Items.Count) return;

            clsMeterTypeComboboxItem mtci = list.Items[e.Index] as clsMeterTypeComboboxItem;
            if (mtci != null)
            {
                Color c = Color.CornflowerBlue;
                int n = MeterManager.GetMeterTXRXType(mtci.MeterType);
                if (n == 0) c = Color.PaleGreen;
                else if (n == 1) c = Color.PaleVioletRed;
                using (SolidBrush sb = new SolidBrush(c))
                    e.Graphics.FillRectangle(sb, new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height));
            }

            Color tc = (e.State & DrawItemState.Selected) != 0 ? Color.White : Color.Black;
            using (SolidBrush sbt = new SolidBrush(tc))
                e.Graphics.DrawString(" " + list.Items[e.Index].ToString(), e.Font, sbt, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private string meterItemGroupIDfromSelected()
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (m == null || mtci == null || !m.HasMeterType(mtci.MeterType)) return "";
            return m.MeterGroupID(mtci.MeterType);
        }

        private MeterType meterItemGroupTypefromSelected()
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (m == null || mtci == null || !m.HasMeterType(mtci.MeterType)) return MeterType.NONE;
            return mtci.MeterType;
        }

        private void updateMeterType()
        {
            if (_ignoreMeterItemChangeEvents || meterItemGroupIDfromSelected() == "") return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            MeterType mt = meterItemGroupTypefromSelected();
            if (m == null || mt == MeterType.NONE) return;

            MeterManager.clsIGSettings igs = m.GetSettingsForMeterGroup(mt);
            if (igs == null) return;

            igs.LowColor = Color.FromArgb(255, clrbtnMeterItemLow.Color);
            igs.HighColor = Color.FromArgb(255, clrbtnMeterItemHigh.Color);
            igs.MarkerColour = Color.FromArgb(255, clrbtnMeterItemIndiciator.Color);
            igs.Colour = Color.FromArgb(255, clrbtnMeterItemHBackground.Color);
            igs.UpdateInterval = (int)nudMeterItemUpdateRate.Value;
            igs.AttackRatio = (float)nudMeterItemAttackRate.Value;
            igs.DecayRatio = (float)nudMeterItemDecayRate.Value;
            igs.ShowHistory = chkMeterItemHistory.Checked;
            igs.HistoryColor = Color.FromArgb(tbMeterItemHistoryAlpha.Value, clrbtnMeterItemHistory.Color);
            igs.Shadow = chkMeterItemShadow.Checked;
            igs.HistoryDuration = (int)nudMeterItemHistoryDuration.Value;
            igs.BarStyle = chkMeterItemSegmented.Checked ? MeterManager.clsBarItem.BarStyle.Segments : MeterManager.clsBarItem.BarStyle.Line;
            igs.SegmentedColour = clrbtnMeterItemSegmentedColour.Color;
            igs.PeakHold = chkMeterItemPeakHold.Checked;
            igs.PeakHoldMarkerColor = Color.FromArgb(255, clrbtnMeterItemPeakHold.Color);
            igs.FadeOnRx = chkMeterItemFadeOnRx.Checked;
            igs.FadeOnTx = chkMeterItemFadeOnTx.Checked;
            igs.ShowType = chkMeterItemTitle.Checked;
            igs.TitleColor = clrbtnMeterItemMeterTitle.Color;
            igs.PeakValue = chkMeterItemPeakValue.Checked;
            igs.PeakValueColour = clrbtnMeterItemPeakValueColour.Color;
            igs.EyeScale = (float)nudMeterItemEyeScale.Value;
            if (mt == MeterType.ANANMM || mt == MeterType.MAGIC_EYE) igs.Average = chkMeterItemSignalAverage.Checked;
            if (mt == MeterType.ANANMM || mt == MeterType.CROSS) igs.DarkMode = chkMeterItemDarkMode.Checked;

            m.ApplySettingsForMeterGroup(mt, igs);
        }

        private void updateItemSettingsControlsForSelected()
        {
            if (meterItemGroupIDfromSelected() == "") return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            MeterType mt = meterItemGroupTypefromSelected();
            if (m == null || mt == MeterType.NONE) return;

            MeterManager.clsIGSettings igs = m.GetSettingsForMeterGroup(mt);
            if (igs == null) return;

            _ignoreMeterItemChangeEvents = true;
            try
            {
                clrbtnMeterItemLow.Color = igs.LowColor;
                clrbtnMeterItemHigh.Color = igs.HighColor;
                clrbtnMeterItemIndiciator.Color = igs.MarkerColour;
                clrbtnMeterItemHBackground.Color = igs.Colour;
                SetNumeric(nudMeterItemUpdateRate, igs.UpdateInterval);
                SetNumeric(nudMeterItemAttackRate, (decimal)igs.AttackRatio);
                SetNumeric(nudMeterItemDecayRate, (decimal)igs.DecayRatio);
                chkMeterItemHistory.Checked = igs.ShowHistory;
                tbMeterItemHistoryAlpha.Value = Math.Max(0, Math.Min(255, (int)igs.HistoryColor.A));
                clrbtnMeterItemHistory.Color = Color.FromArgb(255, igs.HistoryColor);
                SetNumeric(nudMeterItemHistoryDuration, igs.HistoryDuration);
                chkMeterItemSegmented.Checked = igs.BarStyle == MeterManager.clsBarItem.BarStyle.Segments;
                clrbtnMeterItemSegmentedColour.Color = igs.SegmentedColour;
                chkMeterItemPeakHold.Checked = igs.PeakHold;
                clrbtnMeterItemPeakHold.Color = Color.FromArgb(255, igs.PeakHoldMarkerColor);
                chkMeterItemShadow.Checked = igs.Shadow;
                chkMeterItemFadeOnRx.Checked = igs.FadeOnRx;
                chkMeterItemFadeOnTx.Checked = igs.FadeOnTx;
                chkMeterItemTitle.Checked = igs.ShowType;
                clrbtnMeterItemMeterTitle.Color = igs.TitleColor;
                chkMeterItemPeakValue.Checked = igs.PeakValue;
                clrbtnMeterItemPeakValueColour.Color = igs.PeakValueColour;

                bool magic = mt == MeterType.MAGIC_EYE;
                if (magic) SetNumeric(nudMeterItemEyeScale, (decimal)igs.EyeScale);
                nudMeterItemEyeScale.Enabled = magic;
                lblMMEyeSize.Enabled = magic;
                chkMeterItemHistory.Enabled = !magic;
                chkMeterItemPeakHold.Enabled = !(magic || mt == MeterType.CROSS);
                chkMeterItemShadow.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;
                chkMeterItemDarkMode.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;

                bool special = mt == MeterType.ANANMM || mt == MeterType.CROSS || mt == MeterType.MAGIC_EYE;
                chkMeterItemSegmented.Enabled = !special;
                chkMeterItemTitle.Enabled = !special;
                chkMeterItemPeakValue.Enabled = !special;
                lblMMLow.Enabled = !special;
                lblMMHigh.Enabled = !special;
                lblMMBackground.Enabled = !special;
                clrbtnMeterItemLow.Enabled = !special;
                clrbtnMeterItemHigh.Enabled = !special;
                clrbtnMeterItemHBackground.Enabled = !special;

                chkMeterItemSignalAverage.Enabled = mt == MeterType.ANANMM || mt == MeterType.MAGIC_EYE;
                chkMeterItemSignalAverage.Checked = chkMeterItemSignalAverage.Enabled && igs.Average;
                chkMeterItemDarkMode.Checked = chkMeterItemDarkMode.Enabled && igs.DarkMode;

                clrbtnMeterItemHistory.Enabled = chkMeterItemHistory.Enabled && chkMeterItemHistory.Checked;
                tbMeterItemHistoryAlpha.Enabled = chkMeterItemHistory.Enabled && chkMeterItemHistory.Checked;
                clrbtnMeterItemPeakHold.Enabled = chkMeterItemPeakHold.Enabled && chkMeterItemPeakHold.Checked;
                clrbtnMeterItemSegmentedColour.Enabled = chkMeterItemSegmented.Enabled && chkMeterItemSegmented.Checked;
                clrbtnMeterItemMeterTitle.Enabled = chkMeterItemTitle.Enabled && chkMeterItemTitle.Checked;
                clrbtnMeterItemPeakValueColour.Enabled = chkMeterItemPeakValue.Enabled && chkMeterItemPeakValue.Checked;
            }
            finally { _ignoreMeterItemChangeEvents = false; }
        }

        private static void SetNumeric(NumericUpDown n, decimal value)
        {
            if (value < n.Minimum) value = n.Minimum;
            if (value > n.Maximum) value = n.Maximum;
            n.Value = value;
        }

        private void chkMeterItemHistory_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreMeterItemChangeEvents)
            {
                clrbtnMeterItemHistory.Enabled = chkMeterItemHistory.Enabled && chkMeterItemHistory.Checked;
                tbMeterItemHistoryAlpha.Enabled = chkMeterItemHistory.Enabled && chkMeterItemHistory.Checked;
            }
            updateMeterType();
        }

        private void chkMeterItemSegmented_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreMeterItemChangeEvents) clrbtnMeterItemSegmentedColour.Enabled = chkMeterItemSegmented.Enabled && chkMeterItemSegmented.Checked;
            updateMeterType();
        }

        private void chkMeterItemTitle_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreMeterItemChangeEvents) clrbtnMeterItemMeterTitle.Enabled = chkMeterItemTitle.Enabled && chkMeterItemTitle.Checked;
            updateMeterType();
        }

        private void chkMeterItemPeakValue_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreMeterItemChangeEvents) clrbtnMeterItemPeakValueColour.Enabled = chkMeterItemPeakValue.Enabled && chkMeterItemPeakValue.Checked;
            updateMeterType();
        }

        private void chkMeterItemPeakHold_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreMeterItemChangeEvents) clrbtnMeterItemPeakHold.Enabled = chkMeterItemPeakHold.Enabled && chkMeterItemPeakHold.Checked;
            updateMeterType();
        }

        private void anyMeterSettingChanged(object sender, EventArgs e) { updateMeterType(); }
        private void anyMeterColorChanged(object sender, EventArgs e) { updateMeterType(); }

        private void btnMeterCopySettings_Click(object sender, EventArgs e)
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            MeterType mt = meterItemGroupTypefromSelected();
            if (m == null || mt == MeterType.NONE) return;
            _itemGroupSettings = m.GetSettingsForMeterGroup(mt);
            _itemGroupSettingsMeterType = _itemGroupSettings == null ? MeterType.NONE : mt;
            btnMeterPasteSettings.Enabled = canPasteSettings();
        }

        private bool canPasteSettings()
        {
            if (_itemGroupSettings == null || _itemGroupSettingsMeterType == MeterType.NONE) return false;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            MeterType mt = meterItemGroupTypefromSelected();
            if (m == null || mt == MeterType.NONE) return false;

            bool targetSpecial = mt == MeterType.MAGIC_EYE || mt == MeterType.CROSS || mt == MeterType.ANANMM;
            bool sourceSpecial = _itemGroupSettingsMeterType == MeterType.MAGIC_EYE ||
                                 _itemGroupSettingsMeterType == MeterType.CROSS ||
                                 _itemGroupSettingsMeterType == MeterType.ANANMM;
            return (targetSpecial || sourceSpecial) ? mt == _itemGroupSettingsMeterType : true;
        }

        private void btnMeterPasteSettings_Click(object sender, EventArgs e)
        {
            if (!canPasteSettings()) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            MeterType mt = meterItemGroupTypefromSelected();
            if (m == null || mt == MeterType.NONE) return;
            m.ApplySettingsForMeterGroup(mt, _itemGroupSettings);
            updateItemSettingsControlsForSelected();
        }

        private void P27ThetisMetersConfigForm_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
                chkContainerHighlight.Checked = false;
                MeterManager.HighlightContainer("");
                try { console.P27SaveMetersConfiguration(); } catch { }
            }
            else if (!_shutdown) updateMeter2Controls(SelectedContainerId());
        }

        private string SelectedContainerId()
        {
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            return cci == null ? "" : cci.ID;
        }

        private void P27ThetisMetersConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_shutdown && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            MeterManager.HighlightContainer("");
        }

        internal void CloseForShutdown()
        {
            _shutdown = true;
            try { console.P27SaveMetersConfiguration(); } catch { }
            try { Close(); } catch { }
            try { Dispose(); } catch { }
        }
    }

    sealed unsafe public partial class Console
    {
        private P27ThetisMetersConfigForm p27MetersConfigForm;

        internal void P27ShowMetersConfig()
        {
            if (p27MetersConfigForm == null || p27MetersConfigForm.IsDisposed)
                p27MetersConfigForm = new P27ThetisMetersConfigForm(this);

            if (!p27MetersConfigForm.Visible)
                p27MetersConfigForm.Show(this);
            else
            {
                p27MetersConfigForm.WindowState = FormWindowState.Normal;
                p27MetersConfigForm.BringToFront();
                p27MetersConfigForm.Activate();
            }
        }

        internal void P27SaveMetersConfiguration() { P25SaveThetisMeters(); }

        internal void P27CloseMetersConfig()
        {
            P27ThetisMetersConfigForm form = p27MetersConfigForm;
            p27MetersConfigForm = null;
            if (form != null && !form.IsDisposed) form.CloseForShutdown();
        }
    }
}
