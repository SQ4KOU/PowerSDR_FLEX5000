// P24 Common helpers required by the native Thetis meter subsystem.
using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace PowerSDR
{
    public partial class Common
    {
        public const MessageBoxOptions MB_TOPMOST = (MessageBoxOptions)0x00040000L;

        public static bool ShiftKeyDown { get { return (Control.ModifierKeys & Keys.Shift) == Keys.Shift; } }
        public static bool CtrlKeyDown { get { return (Control.ModifierKeys & Keys.Control) == Keys.Control; } }
        public static bool AltlKeyDown { get { return (Control.ModifierKeys & Keys.Alt) == Keys.Alt; } }

        public static void DoubleBuffered(Control control, bool enabled)
        {
            if (control == null) return;
            PropertyInfo p = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (p != null) p.SetValue(control, enabled, null);
        }

        public static void DoubleBufferAll(Control control, bool enabled)
        {
            if (control == null) return;
            DoubleBuffered(control, enabled);
            foreach (Control child in control.Controls) DoubleBufferAll(child, enabled);
        }

        public static int FiveDigitHash(string str)
        {
            if (String.IsNullOrEmpty(str)) return 0;
            uint hash=0;
            foreach(byte b in Encoding.Unicode.GetBytes(str))
            {
                hash += b; hash += (hash << 10); hash ^= (hash >> 6);
            }
            hash += (hash << 3); hash ^= (hash >> 11); hash += (hash << 15);
            return (int)(hash % 99999);
        }

        public static string ColourToString(Color c)
        {
            return c.A+"."+c.R+"."+c.G+"."+c.B;
        }

        public static Color ColourFromString(string s)
        {
            if(String.IsNullOrEmpty(s)) return Color.Empty;
            string[] p=s.Split('.');
            if(p.Length!=4) return Color.Empty;
            int a,r,g,b;
            if(Int32.TryParse(p[0],out a) && Int32.TryParse(p[1],out r) &&
               Int32.TryParse(p[2],out g) && Int32.TryParse(p[3],out b))
                return Color.FromArgb(a,r,g,b);
            return Color.Empty;
        }

        public static double UVfromDBM(double dbm)
        {
            return Math.Sqrt(Math.Pow(10,dbm/10.0)*50.0*1e-3)*1e6;
        }

        public static string SMeterFromDBM_Spaceless(double dbm, bool above)
        {
            int s,over; SMeterFromDBM2(dbm,above,out s,out over);
            return over>0 ? "S9+"+over.ToString() : "S"+s.ToString();
        }

        public static void SMeterFromDBM2(double dbm, bool above, out int s, out int over)
        {
            double s9=above ? -90.0 : -70.0;
            if(dbm<=s9)
            {
                double floor=above ? -144.0 : -124.0;
                s=(int)Math.Floor((dbm-floor)/6.0)+1;
                if(dbm<=floor) s=0;
                if(s<0)s=0; if(s>9)s=9; over=0;
            }
            else
            {
                s=9;
                double d=dbm-s9;
                if(d<4) over=0;
                else if(d<8) over=5;
                else if(d<14) over=10;
                else if(d<18) over=15;
                else if(d<28) over=20;
                else if(d<38) over=30;
                else if(d<48) over=40;
                else if(d<58) over=50;
                else over=60;
            }
        }

        public static int GetLuminance(Color c)
        {
            return (int)(0.2126*c.R + 0.7152*c.G + 0.0722*c.B);
        }

        public static string FourChar(string data1, int data2, Guid guid)
        {
            string input=(data1??"")+":"+data2.ToString()+":"+guid.ToString();
            using(SHA256 sha=SHA256.Create())
            {
                string b64=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
                const string chars="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                int[] idx=new int[4];
                for(int i=0;i<b64.Length;i++) idx[i%4]=(idx[i%4]+b64[i])%chars.Length;
                char[] o=new char[4];
                for(int i=0;i<4;i++) o[i]=chars[idx[i]];
                return new string(o);
            }
        }

        public static string SerializeToBase64<T>(T obj)
        {
            using(MemoryStream ms=new MemoryStream())
            {
                using(GZipStream gz=new GZipStream(ms,CompressionMode.Compress,true))
                {
                    IFormatter f=new BinaryFormatter();
                    f.Serialize(gz,obj);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static T DeserializeFromBase64<T>(string s)
        {
            byte[] b=Convert.FromBase64String(s);
            using(MemoryStream ms=new MemoryStream(b))
            using(GZipStream gz=new GZipStream(ms,CompressionMode.Decompress))
            {
                IFormatter f=new BinaryFormatter();
                return (T)f.Deserialize(gz);
            }
        }

        public static object ConvertToType(object value, Type t)
        {
            if(value==null) return null;
            if(t.IsAssignableFrom(value.GetType())) return value;
            if(t.IsEnum)
            {
                if(value is string) return Enum.Parse(t,(string)value,true);
                return Enum.ToObject(t,Convert.ToInt32(value));
            }
            return Convert.ChangeType(value,t,System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool IsIpv4Valid(string ip)
        {
            System.Net.IPAddress a;
            return System.Net.IPAddress.TryParse(ip,out a) &&
                a.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork;
        }

        public static bool IsValidUri(string uri)
        {
            Uri u; return Uri.TryCreate(uri,UriKind.Absolute,out u);
        }

        public static string GetVerNum()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static void FadeIn(Form f, int milliseconds)
        {
            if(f==null) return;
            f.Opacity=1.0;
        }
    }

    sealed unsafe partial class Console
    {
        public event Action<FormWindowState> WindowStateChangedHandlers;
        public event Action<bool> RX2EnabledChangedHandlers;
        public event Action<bool> MoxChangeHandlers;

        public bool TouchSupport { get { return false; } }

        internal void P24RaiseWindowStateChanged(FormWindowState state)
        {
            Action<FormWindowState> h=WindowStateChangedHandlers; if(h!=null) h(state);
        }
        internal void P24RaiseRX2EnabledChanged(bool enabled)
        {
            Action<bool> h=RX2EnabledChangedHandlers; if(h!=null) h(enabled);
        }
        internal void P24RaiseMoxChanged(bool mox)
        {
            Action<bool> h=MoxChangeHandlers; if(h!=null) h(mox);
        }
    }
}
