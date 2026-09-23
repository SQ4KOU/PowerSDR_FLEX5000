using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace PowerSDR
{
    internal static partial class MeterManager
    {
        private const string P47WindowKeyPrefix = "meterWindowData_";
        private const string P47SchemaKey = "p47PersistenceSchema";

        internal static void P47PreparePersistenceSnapshot()
        {
            lock (_metersLock)
            {
                foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
                {
                    ucMeter uc = kvp.Value;
                    if (uc == null || uc.IsDisposed) continue;

                    // Thetis only updates DockedLocation/DockedSize while its own
                    // drag/resize flags are set. AutoHeight and other programmatic
                    // changes can therefore leave the serialized docked geometry stale.
                    // Before every authoritative save, copy the actual docked geometry.
                    if (!uc.Floating)
                    {
                        uc.DockedLocation = uc.Location;
                        uc.DockedSize = uc.Size;
                    }
                }
            }
        }

        internal static void P47AppendWindowPersistence(ref Dictionary<string, string> settings)
        {
            if (settings == null)
                settings = new Dictionary<string, string>();

            settings[P47SchemaKey] = "1";

            lock (_metersLock)
            {
                foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
                {
                    string id = kvp.Key;
                    frmMeterDisplay form;
                    if (!_lstMeterDisplayForms.TryGetValue(id, out form) ||
                        form == null || form.IsDisposed)
                        continue;

                    Rectangle bounds = form.WindowState == FormWindowState.Normal
                        ? form.Bounds
                        : form.RestoreBounds;

                    int width = Math.Max(form.MinimumSize.Width, bounds.Width);
                    int height = Math.Max(form.MinimumSize.Height, bounds.Height);

                    settings[P47WindowKeyPrefix + id] =
                        bounds.Left.ToString(CultureInfo.InvariantCulture) + "|" +
                        bounds.Top.ToString(CultureInfo.InvariantCulture) + "|" +
                        width.ToString(CultureInfo.InvariantCulture) + "|" +
                        height.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        internal static void P47RestoreWindowPersistence(Dictionary<string, string> settings)
        {
            if (settings == null || settings.Count == 0) return;

            lock (_metersLock)
            {
                foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
                {
                    string id = kvp.Key;
                    string encoded;
                    if (!settings.TryGetValue(P47WindowKeyPrefix + id, out encoded) ||
                        String.IsNullOrEmpty(encoded))
                        continue; // legacy MeterDisplay_<id> table remains the migration fallback

                    frmMeterDisplay form;
                    if (!_lstMeterDisplayForms.TryGetValue(id, out form) ||
                        form == null || form.IsDisposed)
                        continue;

                    Rectangle bounds;
                    if (!P47TryParseBounds(encoded, form.MinimumSize, out bounds))
                        continue;

                    form.StartPosition = FormStartPosition.Manual;
                    form.SetBounds(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
                    Common.ForceFormOnScreen(form);

                    ucMeter uc = kvp.Value;
                    if (uc != null && !uc.IsDisposed && uc.Floating)
                    {
                        // RestoreSettings creates the renderer before the floating form
                        // is finally attached. Keep the ucMeter target size coherent
                        // with the restored form before RunAllRendererDisplays().
                        uc.Size = form.ClientSize;
                    }
                }
            }
        }

        internal static void P47SaveLegacyWindowTables()
        {
            lock (_metersLock)
            {
                foreach (KeyValuePair<string, frmMeterDisplay> kvp in _lstMeterDisplayForms)
                {
                    frmMeterDisplay form = kvp.Value;
                    if (form == null || form.IsDisposed) continue;

                    // Keep the native Thetis/PowerSDR per-form table up to date as
                    // a compatibility and migration fallback. P47's authoritative
                    // geometry is also stored in SQ4KOU_ThetisMeters.
                    Common.SaveForm(form, "MeterDisplay_" + kvp.Key);
                }
            }
        }

        private static bool P47TryParseBounds(string encoded, Size minimum, out Rectangle bounds)
        {
            bounds = Rectangle.Empty;
            string[] parts = encoded.Split('|');
            if (parts.Length != 4) return false;

            int left, top, width, height;
            if (!Int32.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out left) ||
                !Int32.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out top) ||
                !Int32.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out width) ||
                !Int32.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out height))
                return false;

            width = Math.Max(minimum.Width, width);
            height = Math.Max(minimum.Height, height);
            if (width <= 0 || height <= 0) return false;

            bounds = new Rectangle(left, top, width, height);
            return true;
        }

        internal static bool P47AuditPersistenceSnapshot(Dictionary<string, string> settings, out string detail)
        {
            int containers = 0;
            int forms = 0;
            int geometry = 0;

            lock (_metersLock)
            {
                containers = _lstUCMeters.Count;
                forms = _lstMeterDisplayForms.Count;

                foreach (string id in _lstUCMeters.Keys)
                {
                    if (settings != null && settings.ContainsKey(P47WindowKeyPrefix + id))
                        geometry++;
                }
            }

            detail = "containers=" + containers.ToString(CultureInfo.InvariantCulture) +
                     ", forms=" + forms.ToString(CultureInfo.InvariantCulture) +
                     ", geometry=" + geometry.ToString(CultureInfo.InvariantCulture);

            return containers == forms && containers == geometry;
        }
    }
}
