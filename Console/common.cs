//=================================================================
// common.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2003-2013  FlexRadio Systems
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
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace PowerSDR
{
    public partial class Common
    {
        public static void ControlList(Control c, ref ArrayList a)
        {
            if (c.Controls.Count > 0)
            {
                foreach (Control c2 in c.Controls)
                    ControlList(c2, ref a);
            }

            if (c is CheckBox ||
                c is ComboBox ||
                c is NumericUpDown ||
                c is RadioButton ||
                c is TextBox ||
                c is TrackBar ||
                c is ColorButton)
                a.Add(c);
        }

        private static bool P49SerializeFormControl(Control c, ArrayList a)
        {
            if (c == null || String.IsNullOrEmpty(c.Name)) return false;

            ColorButton color = c as ColorButton;
            if (color != null)
            {
                Color clr=color.Color;
                a.Add(c.Name + "/" + clr.R + "." + clr.G + "." + clr.B + "." + clr.A);
                return true;
            }

            CheckBox check=c as CheckBox;
            if(check != null){ a.Add(c.Name + "/" + check.Checked.ToString()); return true; }

            ComboBox combo=c as ComboBox;
            if(combo != null){ a.Add(c.Name + "/" + combo.Text); return true; }

            NumericUpDown numeric=c as NumericUpDown;
            if(numeric != null){
                a.Add(c.Name + "/" + numeric.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return true;
            }

            RadioButton radio=c as RadioButton;
            if(radio != null){ a.Add(c.Name + "/" + radio.Checked.ToString()); return true; }

            TextBox text=c as TextBox;
            if(text != null){ a.Add(c.Name + "/" + text.Text); return true; }

            TrackBar track=c as TrackBar;
            if(track != null){
                a.Add(c.Name + "/" + track.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return true;
            }

            return false;
        }

        private static bool P49RestoreFormControl(Control c, string val)
        {
            if(c == null) return false;

            ColorButton color=c as ColorButton;
            if(color != null)
            {
                string[] parts=val.Split('.');
                int r,g,b,a;
                if(parts.Length==4 &&
                   Int32.TryParse(parts[0],out r) &&
                   Int32.TryParse(parts[1],out g) &&
                   Int32.TryParse(parts[2],out b) &&
                   Int32.TryParse(parts[3],out a))
                {
                    color.Color=Color.FromArgb(a,r,g,b);
                    return true;
                }
                return false;
            }

            CheckBox check=c as CheckBox;
            if(check != null){ bool v; if(Boolean.TryParse(val,out v)){ check.Checked=v; return true; } return false; }

            ComboBox combo=c as ComboBox;
            if(combo != null){ combo.Text=val; return true; }

            NumericUpDown numeric=c as NumericUpDown;
            if(numeric != null)
            {
                decimal v;
                if(!Decimal.TryParse(val,System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out v) &&
                   !Decimal.TryParse(val,out v)) return false;
                if(v>numeric.Maximum)v=numeric.Maximum;
                if(v<numeric.Minimum)v=numeric.Minimum;
                numeric.Value=v;
                return true;
            }

            RadioButton radio=c as RadioButton;
            if(radio != null){ bool v; if(Boolean.TryParse(val,out v)){ radio.Checked=v; return true; } return false; }

            TextBox text=c as TextBox;
            if(text != null){ text.Text=val; return true; }

            TrackBar track=c as TrackBar;
            if(track != null)
            {
                int v;
                if(!Int32.TryParse(val,System.Globalization.NumberStyles.Integer,System.Globalization.CultureInfo.InvariantCulture,out v) &&
                   !Int32.TryParse(val,out v)) return false;
                if(v>track.Maximum)v=track.Maximum;
                if(v<track.Minimum)v=track.Minimum;
                track.Value=v;
                return true;
            }

            return false;
        }
        public static void SaveForm(Form form, string tablename)
        {
            ArrayList a = new ArrayList();
            ArrayList temp = new ArrayList();
            ControlList(form, ref temp);

            foreach(Control c in temp)
                P49SerializeFormControl(c,a);

            Rectangle bounds = form.WindowState == FormWindowState.Normal
                ? form.Bounds
                : form.RestoreBounds;

            a.Add("Top/" + bounds.Top.ToString(System.Globalization.CultureInfo.InvariantCulture));
            a.Add("Left/" + bounds.Left.ToString(System.Globalization.CultureInfo.InvariantCulture));
            a.Add("Width/" + bounds.Width.ToString(System.Globalization.CultureInfo.InvariantCulture));
            a.Add("Height/" + bounds.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
            a.Add("WindowState/" + (form.WindowState == FormWindowState.Maximized ? "Maximized" : "Normal"));

            DB.ReplaceVars(tablename, ref a);
        }

        public static void RestoreForm(Form form, string tablename, bool restore_size)
        {
            ArrayList temp = new ArrayList();
            ControlList(form, ref temp);

            System.Collections.Generic.Dictionary<string,Control> controls =
                new System.Collections.Generic.Dictionary<string,Control>(StringComparer.Ordinal);

            foreach(Control c in temp)
            {
                if(c != null && !String.IsNullOrEmpty(c.Name))
                    controls[c.Name]=c;
            }

            ArrayList a=DB.GetVars(tablename);
            string windowState="Normal";

            foreach(string s in a)
            {
                if(String.IsNullOrEmpty(s)) continue;
                int slash=s.IndexOf('/');
                if(slash <= 0) continue;

                string name=s.Substring(0,slash);
                string val=s.Substring(slash+1);
                int n;

                if(name=="Top")
                {
                    if(Int32.TryParse(val,out n)){ form.StartPosition=FormStartPosition.Manual; form.Top=n; }
                    continue;
                }
                if(name=="Left")
                {
                    if(Int32.TryParse(val,out n)){ form.StartPosition=FormStartPosition.Manual; form.Left=n; }
                    continue;
                }
                if(name=="Width")
                {
                    if(restore_size && Int32.TryParse(val,out n) && n>0) form.Width=n;
                    continue;
                }
                if(name=="Height")
                {
                    if(restore_size && Int32.TryParse(val,out n) && n>0) form.Height=n;
                    continue;
                }
                if(name=="WindowState")
                {
                    windowState=val;
                    continue;
                }

                Control control;
                if(controls.TryGetValue(name,out control))
                {
                    if(!P49RestoreFormControl(control,val))
                        Debug.WriteLine("P49 form restore rejected value: " + tablename + "." + name + "=" + val);
                }
            }

            ForceFormOnScreen(form);
            if(restore_size && String.Equals(windowState,"Maximized",StringComparison.OrdinalIgnoreCase))
                form.WindowState=FormWindowState.Maximized;
        }

        public static void ForceFormOnScreen(Form f)
        {
            Screen[] screens = Screen.AllScreens;
            bool on_screen = false;

            int left = 0, right = 0, top = 0, bottom = 0;

            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i].Bounds.Left < left)
                    left = screens[i].Bounds.Left;

                if (screens[i].Bounds.Top < top)
                    top = screens[i].Bounds.Top;

                if (screens[i].Bounds.Bottom > bottom)
                    bottom = screens[i].Bounds.Bottom;

                if (screens[i].Bounds.Right > right)
                    right = screens[i].Bounds.Right;
            }

            if (f.Left > left &&
                f.Top > top &&
                f.Right < right &&
                f.Bottom < bottom)
                on_screen = true;

            if (!on_screen)
            {
                //f.Location = new Point(0, 0);

                if (f.Left < left)
                    f.Left = left;

                if (f.Top < top)
                    f.Top = top;

                if (f.Bottom > bottom)
                {
                    if ((f.Top - (f.Bottom - bottom)) >= top)
                        f.Top -= (f.Bottom - bottom);
                    else f.Top = 0;
                }

                if (f.Right > right)
                {
                    if ((f.Left - (f.Right - right)) >= left)
                        f.Left -= (f.Right - right);
                    else f.Left = 0;
                }
            }
        }

        public static void TabControlInsert(TabControl tc, TabPage tp, int index)
        {
            tc.SuspendLayout();
            // temp storage to rearrange tabs
            TabPage[] temp = new TabPage[tc.TabPages.Count + 1];

            // copy pages in order and insert new page when needed
            for (int i = 0; i < tc.TabPages.Count + 1; i++)
            {
                if (i < index) temp[i] = tc.TabPages[i];
                else if (i == index) temp[i] = tp;
                else if (i > index) temp[i] = tc.TabPages[i - 1];
            }

            // erase all tab pages
            while (tc.TabPages.Count > 0)
                tc.TabPages.RemoveAt(0);

            // add them back with new page inserted
            for (int i = 0; i < temp.Length; i++)
                tc.TabPages.Add(temp[i]);

            tc.ResumeLayout();
        }

        public static string[] SortedComPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Array.Sort<string>(ports, delegate (string strA, string strB)
            {
                try
                {
                    int idA = int.Parse(strA.Substring(3));
                    int idB = int.Parse(strB.Substring(3));

                    return idA.CompareTo(idB);
                }
                catch (Exception)
                {
                    return strA.CompareTo(strB);
                }
            });
            return ports;
        }

        public static string RevToString(uint rev)
        {
            return ((byte)(rev >> 24)).ToString() + "." +
                ((byte)(rev >> 16)).ToString() + "." +
                ((byte)(rev >> 8)).ToString() + "." +
                ((byte)(rev >> 0)).ToString();
        }
    }
}