using System;
using System.Drawing;
using System.Windows.Forms;

namespace PowerSDR
{
    public partial class Setup
    {
        private TabPage flexMetersAppearancePage;
        private FlexMeters.FlexMetersEditorForm flexMetersEditorForm;

        private void InitializeFlexMetersAppearanceEntry()
        {
            if (flexMetersAppearancePage != null)
                return;

            flexMetersAppearancePage = new TabPage();
            flexMetersAppearancePage.Name = "tpAppearanceFlexMeters";
            flexMetersAppearancePage.Text = "Meters/Gadgets";
            flexMetersAppearancePage.BackColor = SystemColors.Control;
            flexMetersAppearancePage.AutoScroll = true;

            // Thetis presents grpMultiMeterHolder directly on the Appearance
            // page.  Host the ported 726x411 surface directly instead of
            // inserting an extra launcher page/window.
            FlexMeters.MeterWorkspaceManager manager =
                console.EnsureFlexMetersWorkspaceManager();

            flexMetersEditorForm =
                new FlexMeters.FlexMetersEditorForm(
                    manager,
                    delegate(System.Guid id)
                    {
                        console.RecoverFlexMetersContainer(id);
                    });
            flexMetersEditorForm.TopLevel = false;
            flexMetersEditorForm.FormBorderStyle =
                FormBorderStyle.None;
            flexMetersEditorForm.StartPosition =
                FormStartPosition.Manual;
            flexMetersEditorForm.Location = new Point(0, 0);
            flexMetersEditorForm.MinimumSize = new Size(726, 411);
            flexMetersEditorForm.Size = new Size(726, 411);

            flexMetersAppearancePage.Controls.Add(
                flexMetersEditorForm);
            flexMetersEditorForm.Show();

            tcAppearance.TabPages.Add(flexMetersAppearancePage);
        }
    }
}
