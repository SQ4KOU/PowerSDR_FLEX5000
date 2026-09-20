using System;
using System.Drawing;
using System.IO;

namespace PowerSDR
{
    internal static class P24MeterResources
    {
        private static readonly string Root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "P24ThetisMeterResources");

        internal static Image Get(string name)
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

        internal static Image gear { get { return Get("gear"); } }
        internal static Image pin_not_on_top { get { return Get("pin_not_on_top"); } }
        internal static Image pin_on_top { get { return Get("pin_on_top"); } }
        internal static Image dot { get { return Get("dot"); } }
        internal static Image dockIcon_dock { get { return Get("dockIcon_dock"); } }
        internal static Image dockIcon_float { get { return Get("dockIcon_float"); } }
        internal static Image resizegrab { get { return Get("resizegrab"); } }
        internal static Image arrow_left { get { return Get("arrow_left"); } }
        internal static Image arrow_topleft { get { return Get("arrow_topleft"); } }
        internal static Image arrow_up { get { return Get("arrow_up"); } }
        internal static Image arrow_topright { get { return Get("arrow_topright"); } }
        internal static Image arrow_right { get { return Get("arrow_right"); } }
        internal static Image arrow_bottomright { get { return Get("arrow_bottomright"); } }
        internal static Image down { get { return Get("down"); } }
        internal static Image arrow_bottomleft { get { return Get("arrow_bottomleft"); } }
    }
}
