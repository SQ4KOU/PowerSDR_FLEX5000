/*  ucOtherButtonsOptionsGrid.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2026 Richard Samphire MW0LGE

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at

mw0lge@grange-lane.co.uk
*/
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace PowerSDR
{
public partial class ucOtherButtonsOptionsGrid : UserControl
    {
        public class MacroButtonEventArgs : EventArgs
        {
            public OtherButtonId Id { get; }
            public int BitGroup { get; }
            public int BitNumber { get; }
            public bool IsChecked { get; }
            public ButtonTS Button { get; }
            public CheckBoxTS CheckBox { get; }

            public MacroButtonEventArgs(OtherButtonId id, int bitGroup, int bitNumber, ButtonTS button, CheckBoxTS checkBox, bool isChecked)
            {
                Id = id;
                BitGroup = bitGroup;
                BitNumber = bitNumber;
                Button = button;
                CheckBox = checkBox;
                IsChecked = isChecked;
            }
        }

        private List<CheckBoxTS> _check_boxes;
        private List<ButtonTS> _buttons;

        private bool _init;
        public event EventHandler CheckboxChanged;
        public event EventHandler<MacroButtonEventArgs> MacroSetupClicked;

        private Dictionary<OtherButtonId, CheckBoxTS> _checkbox_by_id;
        private Dictionary<int, List<(int bit, CheckBoxTS cb)>> _checkbox_by_group;

        private TableLayoutPanel _table;

        private ToolTip _tooltip;

        private OtherButtonMacroSettings[] _macro_settings;

        public ucOtherButtonsOptionsGrid()
        {
            _macro_settings = new OtherButtonMacroSettings[OtherButtonIdHelpers.MACRO_BUTTONS_PERGROUP + 1];
            for (int n = 0; n < _macro_settings.Length; n++)
            {
                _macro_settings[n] = new OtherButtonMacroSettings();
                _macro_settings[n].Number = n;
            }

            _init = false;
            InitializeComponent();

            this.Size = new Size(173, 182);
            this.scrollableControl1.Location = new Point(0, 0);
            this.scrollableControl1.Size = new Size(170, 178);
            this.scrollableControl1.AutoScroll = true;

            _tooltip = new ToolTip();
            _tooltip.AutomaticDelay = 300;
            _tooltip.AutoPopDelay = 8000;
            _tooltip.InitialDelay = 500;
            _tooltip.ReshowDelay = 100;
            _tooltip.ShowAlways = true;

            _check_boxes = new List<CheckBoxTS>();
            _buttons = new List<ButtonTS>();

            _checkbox_by_id = new Dictionary<OtherButtonId, CheckBoxTS>();
            _checkbox_by_group = new Dictionary<int, List<(int bit, CheckBoxTS cb)>>();

            initialise_checkboxes();
        }

        private void initialise_checkboxes()
        {
            _init = false;

            for (int i = 0; i < _check_boxes.Count; i++)
                _check_boxes[i].CheckedChanged -= checkbox_checked_changed;
            for (int i = 0; i < _buttons.Count; i++)
                _buttons[i].Click -= button_clicked;

            _check_boxes.Clear();
            _buttons.Clear();
            _checkbox_by_id.Clear();
            _checkbox_by_group.Clear();

            int btn_w = 24;
            int pad_l = 0;
            int pad_r = 2;
            int btn_col_w = btn_w + pad_l + pad_r;

            if (_table == null)
            {
                _table = new TableLayoutPanel();
                _table.Name = "tbl_other_buttons";
                _table.AutoSize = false;
                _table.Dock = DockStyle.Top;
                _table.ColumnCount = 4;
                _table.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
                _table.ColumnStyles.Clear();
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btn_col_w));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btn_col_w));
                _table.Padding = new Padding(0, 0, pad_r, 0);
                scrollableControl1.Controls.Add(_table);
            }
            else
            {
                _table.SuspendLayout();
                _table.Controls.Clear();
                _table.RowStyles.Clear();
                _table.ColumnStyles.Clear();
                _table.ColumnCount = 4;
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btn_col_w));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btn_col_w));
                _table.Padding = new Padding(0, 0, pad_r, 0);
                _table.RowCount = 0;
                _table.ResumeLayout(false);
            }

            this.SuspendLayout();
            scrollableControl1.SuspendLayout();
            _table.SuspendLayout();
            bool was_visible = scrollableControl1.Visible;
            scrollableControl1.Visible = false;
            _table.Width = scrollableControl1.ClientSize.Width;

            int row = 0;
            int col = 0;
            (OtherButtonId id, int bit_group, int bit_number, string caption, string icon_on, string icon_off, string tooltip)[] data = OtherButtonIdHelpers.CheckBoxData;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].id == OtherButtonId.INFO_TEXT)
                {
                    if (col != 0)
                    {
                        col = 0;
                        row++;
                    }

                    LabelTS lbl = new LabelTS();
                    lbl.Name = "lbl_" + i.ToString();
                    lbl.AutoSize = true;
                    lbl.Margin = new Padding(0, 2, 0, 0);
                    lbl.Font = new Font(Font, FontStyle.Bold);
                    lbl.Text = data[i].caption;
                    _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    _table.Controls.Add(lbl, 0, row);
                    _table.SetColumnSpan(lbl, 4);
                    _table.RowCount = row + 1;
                    row++;
                    continue;
                }

                if (data[i].id == OtherButtonId.SPLITTER)
                {
                    if (col != 0)
                    {
                        col = 0;
                        row++;
                    }

                    PanelTS sep = new PanelTS();
                    sep.Name = "sep_" + i.ToString();
                    sep.Height = 1;
                    sep.Dock = DockStyle.Fill;
                    sep.Margin = new Padding(0, 2, 0, 4);
                    sep.BackColor = SystemColors.ControlDark;
                    _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    _table.Controls.Add(sep, 0, row);
                    _table.SetColumnSpan(sep, 4);
                    _table.RowCount = row + 1;
                    row++;
                    continue;
                }

                CheckBoxTS chk = new CheckBoxTS();
                chk.Name = "chkOtherButton_" + ((int)data[i].id).ToString();
                chk.AutoSize = false;
                chk.TextAlign = ContentAlignment.MiddleLeft;
                chk.AutoEllipsis = true;
                chk.Margin = new Padding(0, 0, 0, 0);
                chk.Text = OtherButtonIdHelpers.OtherButtonIDToText(data[i].id);
                chk.Tag = new ValueTuple<OtherButtonId, int, int>(data[i].id, data[i].bit_group, data[i].bit_number);
                _tooltip.SetToolTip(chk, data[i].tooltip);
                Size sz = chk.GetPreferredSize(Size.Empty);
                int chk_h = sz.Height;
                chk.MinimumSize = new Size(0, chk_h);
                chk.Height = chk_h;
                chk.Dock = DockStyle.Fill;
                chk.CheckedChanged += checkbox_checked_changed;

                bool is_macro = data[i].id >= OtherButtonId._MACRO_0 && data[i].id <= OtherButtonId._MACRO_30;

                ButtonTS but = null;
                if (is_macro)
                {
                    but = new ButtonTS();
                    but.Name = "btnOtherButtonMacroButton_" + ((int)data[i].id).ToString();
                    but.AutoSize = false;
                    but.Size = new Size(btn_w, chk_h);
                    but.MinimumSize = new Size(btn_w, chk_h);
                    but.MaximumSize = new Size(btn_w, chk_h);
                    but.TextAlign = ContentAlignment.MiddleCenter;
                    but.Margin = new Padding(pad_l, 0, pad_r, 0);
                    but.Anchor = AnchorStyles.Left;
                    but.Text = "...";
                    but.Tag = new ValueTuple<OtherButtonId, int, int>(data[i].id, data[i].bit_group, data[i].bit_number);
                    _tooltip.SetToolTip(but, data[i].tooltip);
                    but.Click += button_clicked;
                }

                if (col == 0)
                {
                    _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    _table.RowCount = row + 1;
                }

                int base_col = col == 0 ? 0 : 2;
                _table.Controls.Add(chk, base_col, row);
                if (!is_macro) _table.SetColumnSpan(chk, 2);
                _check_boxes.Add(chk);
                _checkbox_by_id[data[i].id] = chk;

                if (is_macro)
                {
                    _table.Controls.Add(but, base_col + 1, row);
                    _buttons.Add(but);
                }

                if (data[i].bit_group >= 0 && data[i].bit_number >= 0)
                {
                    List<(int bit, CheckBoxTS cb)> list;
                    if (!_checkbox_by_group.TryGetValue(data[i].bit_group, out list))
                    {
                        list = new List<(int bit, CheckBoxTS cb)>();
                        _checkbox_by_group[data[i].bit_group] = list;
                    }
                    list.Add((data[i].bit_number, chk));
                }

                col++;
                if (col > 1)
                {
                    col = 0;
                    row++;
                }
            }

            _table.Height = _table.PreferredSize.Height;

            scrollableControl1.Visible = was_visible;
            _table.ResumeLayout(true);
            scrollableControl1.ResumeLayout(true);
            this.ResumeLayout(true);

            _init = true;
        }
        private void checkbox_checked_changed(object sender, EventArgs e)
        {
            if (!_init) return;
            if (CheckboxChanged != null) CheckboxChanged(this, EventArgs.Empty);
        }
        private void button_clicked(object sender, EventArgs e)
        {
            if (!_init) return;
            ButtonTS btn = (ButtonTS)sender;
            ValueTuple<OtherButtonId, int, int> meta = (ValueTuple<OtherButtonId, int, int>)btn.Tag;
            CheckBoxTS cb;
            _checkbox_by_id.TryGetValue(meta.Item1, out cb);
            bool is_checked = cb != null && cb.Checked;
            MacroButtonEventArgs args = new MacroButtonEventArgs(meta.Item1, meta.Item2, meta.Item3, btn, cb, is_checked);
            if (MacroSetupClicked != null) MacroSetupClicked(this, args);
        }
        public int GetBitfield(int bit_group)
        {
            int bitfield_value = 0;
            List<(int bit, CheckBoxTS cb)> list;
            if (!_checkbox_by_group.TryGetValue(bit_group, out list)) return 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].cb.Checked) bitfield_value |= (1 << list[i].bit);
            }
            return bitfield_value;
        }

        public void SetBitfield(int bit_group, int value)
        {
            bool old_init = _init;
            _init = false;
            List<(int bit, CheckBoxTS cb)> list;
            if (_checkbox_by_group.TryGetValue(bit_group, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    bool is_checked = (value & (1 << list[i].bit)) != 0;
                    list[i].cb.Checked = is_checked;
                }
            }
            _init = old_init;
        }

        public int GetCheckedCount(int bit_group)
        {
            int count = 0;
            List<(int bit, CheckBoxTS cb)> list;
            if (!_checkbox_by_group.TryGetValue(bit_group, out list)) return 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].cb.Checked) count++;
            }
            return count;
        }

        public OtherButtonMacroSettings GetMacroSettings(int macro)
        {
            if (macro < 0 || macro > _macro_settings.Length - 1) return null;
            return _macro_settings[macro];
        }

        public void SetMacroSettings(int macro, OtherButtonMacroSettings settings)
        {
            if (macro < 0 || macro > _macro_settings.Length - 1) return;
            _macro_settings[macro] = new OtherButtonMacroSettings(settings);
        }
    }
}
