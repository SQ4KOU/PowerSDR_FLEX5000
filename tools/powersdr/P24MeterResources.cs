using System;
using System.Drawing;
using System.IO;

namespace PowerSDR
{
    internal static class P24MeterResources
    {
        private static readonly string Root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "P24ThetisMeterResources");

        internal static Bitmap Get(string name)
        {
            try
            {
                string p = Path.Combine(Root, name + ".png");
                if (!File.Exists(p)) return null;
                using (FileStream fs = new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image img = Image.FromStream(fs))
                    return new Bitmap(img);
            }
            catch { return null; }
        }

        internal static Bitmap gear { get { return Get("gear"); } }
        internal static Bitmap pin_not_on_top { get { return Get("pin_not_on_top"); } }
        internal static Bitmap pin_on_top { get { return Get("pin_on_top"); } }
        internal static Bitmap dot { get { return Get("dot"); } }
        internal static Bitmap dockIcon_dock { get { return Get("dockIcon_dock"); } }
        internal static Bitmap dockIcon_float { get { return Get("dockIcon_float"); } }
        internal static Bitmap resizegrab { get { return Get("resizegrab"); } }
        internal static Bitmap arrow_left { get { return Get("arrow_left"); } }
        internal static Bitmap arrow_topleft { get { return Get("arrow_topleft"); } }
        internal static Bitmap arrow_up { get { return Get("arrow_up"); } }
        internal static Bitmap arrow_topright { get { return Get("arrow_topright"); } }
        internal static Bitmap arrow_right { get { return Get("arrow_right"); } }
        internal static Bitmap arrow_bottomright { get { return Get("arrow_bottomright"); } }
        internal static Bitmap down { get { return Get("down"); } }
        internal static Bitmap arrow_bottomleft { get { return Get("arrow_bottomleft"); } }
    }
}
