using System;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalServicesApp
{

    public class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            // ---- Form setup ---
            Text = "Municipal Services - Main Menu";
            StartPosition = FormStartPosition.CenterScreen;

            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(820, 600);
            MinimumSize = new Size(640, 500); 
            WindowState = FormWindowState.Maximized; 
            BackColor = UiTheme.Background;
            Font = UiTheme.LabelFont;
            ForeColor = UiTheme.TextDark;


            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = UiTheme.Primary
            };

            var lblSubTitle = new Label
            {
                Text = "Republic of South Africa - Citizen Portal",
                Font = UiTheme.SubtitleFont,
                ForeColor = UiTheme.PrimaryLight,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            };

            var lblTitle = new Label
            {
                Text = "Municipal Services",
                Font = UiTheme.TitleFont,
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 62,
                TextAlign = ContentAlignment.BottomCenter
            };

            header.Controls.Add(lblSubTitle);
            header.Controls.Add(lblTitle);


            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(90, 28, 90, 28)
            };
            for (int i = 0; i < 4; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            }
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var btnReportIssues = CreateMenuButton("Report Issues", true);
            var btnEvents = CreateMenuButton("Local Events && Announcements", true);
            var btnStatus = CreateMenuButton("Service Request Status  (coming soon)", false);

            btnReportIssues.Click += BtnReportIssues_Click;
            btnEvents.Click += BtnEvents_Click;

            var btnExit = new Button
            {
                Text = "Exit",
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                BackColor = UiTheme.Surface,
                ForeColor = UiTheme.TextMuted,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 10, 0, 0)
            };
            btnExit.FlatAppearance.BorderColor = Color.FromArgb(210, 216, 222);
            btnExit.Click += delegate { Close(); };

            layout.Controls.Add(btnReportIssues, 0, 0);
            layout.Controls.Add(btnEvents, 0, 1);
            layout.Controls.Add(btnStatus, 0, 2);
            layout.Controls.Add(btnExit, 0, 3);

            Controls.Add(layout);
            Controls.Add(header);
        }
        private static Button CreateMenuButton(string text, bool enabled)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = UiTheme.ButtonFont,
                FlatStyle = FlatStyle.Flat,
                Enabled = enabled,
                Margin = new Padding(0, 6, 0, 6),
                Cursor = enabled ? Cursors.Hand : Cursors.No
            };
            button.FlatAppearance.BorderSize = 0;

            if (enabled)
            {
                button.BackColor = UiTheme.Accent;
                button.ForeColor = Color.White;
            }
            else
            {
                button.BackColor = Color.FromArgb(224, 229, 234);
                button.ForeColor = UiTheme.TextMuted;
            }
            return button;
        }
        private void BtnReportIssues_Click(object sender, EventArgs e)
        {
            Hide();
            using (var reportForm = new ReportIssuesForm())
            {
                reportForm.ShowDialog(this);
            }
            Show();
        }
        private void BtnEvents_Click(object sender, EventArgs e)
        {
            Hide();
            using (var eventsForm = new LocalEventsForm())
            {
                eventsForm.ShowDialog(this);
            }
            Show();
        }
    }
}
