using System;
using System.Drawing;
using System.Windows.Forms;

namespace PowerSDR
{
    public partial class Setup
    {
        private TabPage flexMetersAppearancePage;
        private Button flexMetersOpenEditorButton;
        private Label flexMetersAppearanceStatus;
        private FlexMeters.FlexMetersEditorForm flexMetersEditorForm;

        private void InitializeFlexMetersAppearanceEntry()
        {
            if (flexMetersAppearancePage != null)
                return;

            flexMetersAppearancePage = new TabPage();
            flexMetersAppearancePage.Name = "tpAppearanceFlexMeters";
            flexMetersAppearancePage.Text = "Meters/Gadgets";
            flexMetersAppearancePage.BackColor = SystemColors.Control;

            var heading = new Label();
            heading.Location = new Point(20, 24);
            heading.Size = new Size(545, 42);
            heading.Font = new Font(
                heading.Font.FontFamily,
                11.0f,
                FontStyle.Bold);
            heading.Text = "Thetis Meters/Gadgets - FLEX-5000 native integration";
            flexMetersAppearancePage.Controls.Add(heading);

            flexMetersOpenEditorButton = new Button();
            flexMetersOpenEditorButton.Location = new Point(20, 82);
            flexMetersOpenEditorButton.Size = new Size(180, 40);
            flexMetersOpenEditorButton.Text = "Open Meters/Gadgets...";
            flexMetersOpenEditorButton.Click += FlexMetersOpenEditorButtonClick;
            flexMetersAppearancePage.Controls.Add(flexMetersOpenEditorButton);

            flexMetersAppearanceStatus = new Label();
            flexMetersAppearanceStatus.Location = new Point(20, 140);
            flexMetersAppearanceStatus.Size = new Size(545, 64);
            flexMetersAppearancePage.Controls.Add(flexMetersAppearanceStatus);

            var layoutNote = new Label();
            layoutNote.Location = new Point(20, 220);
            layoutNote.Size = new Size(545, 72);
            layoutNote.Text =
                "The editor opens in a separate resizable window. " +
                "The original Thetis Meters/Gadgets workspace is 724x410, " +
                "while the legacy KE9NS Appearance page is 592x318. " +
                "Keeping the editor separate prevents overlap with existing Setup controls.";
            flexMetersAppearancePage.Controls.Add(layoutNote);

            flexMetersAppearancePage.Enter += delegate
            {
                UpdateFlexMetersAppearanceStatus();
            };

            tcAppearance.TabPages.Add(flexMetersAppearancePage);
            UpdateFlexMetersAppearanceStatus();
        }

        private void FlexMetersOpenEditorButtonClick(object sender, EventArgs e)
        {
            FlexMeters.MeterWorkspaceManager manager =
                console.EnsureFlexMetersWorkspaceManager();

            if (flexMetersEditorForm == null ||
                flexMetersEditorForm.IsDisposed)
            {
                flexMetersEditorForm =
                    new FlexMeters.FlexMetersEditorForm(manager);
                flexMetersEditorForm.FormClosed += delegate
                {
                    flexMetersEditorForm = null;
                    UpdateFlexMetersAppearanceStatus();
                };
                flexMetersEditorForm.Show(console);
            }
            else
            {
                flexMetersEditorForm.BringToFront();
                flexMetersEditorForm.Activate();
            }

            UpdateFlexMetersAppearanceStatus();
        }

        private void UpdateFlexMetersAppearanceStatus()
        {
            if (flexMetersAppearanceStatus == null || console == null)
                return;

            FlexMeters.MeterWorkspaceManager manager =
                console.EnsureFlexMetersWorkspaceManager();

            flexMetersAppearanceStatus.Text =
                "Containers: " + manager.ContainerCount.ToString() +
                "\r\n" +
                "Implemented native items: Signal Peak, Signal Text (RX1)";
        }
    }
}
