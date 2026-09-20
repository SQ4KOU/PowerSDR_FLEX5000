using System;
using System.Drawing;
using System.IO;

namespace PowerSDR
{
    internal static class P24MeterResources
    {
        private static readonly string Root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "P24ThetisMeterResources");

        private static Image Load(string name)
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

        internal static Image gear { get { return Load("gear"); } }
        internal static Image pin_not_on_top { get { return Load("pin_not_on_top"); } }
        internal static Image pin_on_top { get { return Load("pin_on_top"); } }
        internal static Image dot { get { return Load("dot"); } }
        internal static Image dockIcon_dock { get { return Load("dockIcon_dock"); } }
        internal static Image dockIcon_float { get { return Load("dockIcon_float"); } }
        internal static Image resizegrab { get { return Load("resizegrab"); } }
        internal static Image arrow_left { get { return Load("arrow_left"); } }
        internal static Image arrow_topleft { get { return Load("arrow_topleft"); } }
        internal static Image arrow_up { get { return Load("arrow_up"); } }
        internal static Image arrow_topright { get { return Load("arrow_topright"); } }
        internal static Image arrow_right { get { return Load("arrow_right"); } }
        internal static Image arrow_bottomright { get { return Load("arrow_bottomright"); } }
        internal static Image down { get { return Load("down"); } }
        internal static Image arrow_bottomleft { get { return Load("arrow_bottomleft"); } }
    }
}
