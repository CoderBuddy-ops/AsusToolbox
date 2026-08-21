using System.Reflection;
using Asus.UI;

namespace Asus
{
    /// <summary>
    /// Minimal About page: identity, version and open-source attribution.
    /// Kept deliberately compact — no marketing, no branding clutter.
    /// </summary>
    public class About : RForm
    {
        public About()
        {
            Text = "About Asus";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            InitTheme(true);
            ClientSize = new Size(360, 250);

            BuildLayout();

            Shown += (s, e) => FormPosition();
        }

        public void FormPosition()
        {
            Top = Program.settingsForm.Top;
            Left = Program.settingsForm.Left - Width - 5;

            if (Left < 0)
                Left = Program.settingsForm.Left + Program.settingsForm.Width + 5;
        }

        void BuildLayout()
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 18, 24, 18),
                ColumnCount = 1,
                RowCount = 5
            };
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));   // logo
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));   // name
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));   // tagline
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));   // version
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // attribution

            var logo = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.standard.ToBitmap()
            };

            var name = new Label
            {
                Text = "Asus",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            };

            var tagline = new Label
            {
                Text = "Vivobook 15 Control Utility",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var version = new Label
            {
                Text = "Version " + Assembly.GetExecutingAssembly().GetName().Version,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(128, Color.Gray)
            };

            var attribution = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Segoe UI", 8F),
                Text = "Open-source project · GNU GPL v3\nIndependent — not an official ASUS Corporation product\nDerived from the GHelper project by Seerge"
            };

            table.Controls.Add(logo, 0, 0);
            table.Controls.Add(name, 0, 1);
            table.Controls.Add(tagline, 0, 2);
            table.Controls.Add(version, 0, 3);
            table.Controls.Add(attribution, 0, 4);

            Controls.Add(table);
        }
    }
}
