using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Local Events and Announcements screen (Part 2).
    /// </summary>
    public class LocalEventsForm : Form
    {
        private readonly TextBox _txtSearch;
        private readonly ComboBox _cmbCategory;
        private readonly ListView _lvEvents;
        private readonly RichTextBox _rtbDetails;
        private readonly Label _lblCount;

        public LocalEventsForm()
        {
            Text = "Local Events and Announcements";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(1100, 720);
            MinimumSize = new Size(900, 620);
            WindowState = FormWindowState.Maximized;
            BackColor = UiTheme.Background;
            Font = UiTheme.LabelFont;
            ForeColor = UiTheme.TextDark;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = UiTheme.Primary
            };
            header.Controls.Add(new Label
            {
                Text = "Local Events and Announcements",
                Font = UiTheme.TitleFont,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));

            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            searchPanel.Controls.Add(new Label
            {
                Text = "Search:",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 9, 0, 0)
            });
            _txtSearch = new TextBox
            {
                Width = 270,
                Font = UiTheme.LabelFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearch.TextChanged += FilterChanged;
            searchPanel.Controls.Add(_txtSearch);

            searchPanel.Controls.Add(new Label
            {
                Text = "  Category:",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 9, 0, 0)
            });
            _cmbCategory = new ComboBox
            {
                Width = 210,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.LabelFont
            };
            _cmbCategory.SelectedIndexChanged += FilterChanged;
            searchPanel.Controls.Add(_cmbCategory);

            var btnReset = new Button
            {
                Text = "Reset",
                Width = 90,
                Height = 30,
                BackColor = UiTheme.Surface,
                ForeColor = UiTheme.TextDark,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(190, 199, 205);
            btnReset.Click += BtnReset_Click;
            searchPanel.Controls.Add(btnReset);

            _lvEvents = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false,
                GridLines = true,
                Font = UiTheme.LabelFont,
                BackColor = Color.White
            };
            _lvEvents.Columns.Add("Date", 150);
            _lvEvents.Columns.Add("Type", 120);
            _lvEvents.Columns.Add("Title", 270);
            _lvEvents.Columns.Add("Venue", 210);
            _lvEvents.SelectedIndexChanged += LvEvents_SelectedIndexChanged;

            _rtbDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UiTheme.LabelFont,
                Text = "Select an event or announcement to view its details."
            };

            var bottomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            _lblCount = new Label
            {
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = UiTheme.TextMuted,
                Padding = new Padding(0, 12, 18, 0)
            };
            var btnBack = new Button
            {
                Text = "Back to Main Menu",
                Width = 180,
                Height = 38,
                Font = UiTheme.ButtonFont,
                BackColor = UiTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += delegate { Close(); };
            bottomPanel.Controls.Add(btnBack);
            bottomPanel.Controls.Add(_lblCount);

            root.Controls.Add(searchPanel, 0, 0);
            root.SetColumnSpan(searchPanel, 2);
            root.Controls.Add(_lvEvents, 0, 1);
            root.Controls.Add(_rtbDetails, 1, 1);
            root.Controls.Add(bottomPanel, 0, 2);
            root.SetColumnSpan(bottomPanel, 2);

            Controls.Add(root);
            Controls.Add(header);

            LoadCategories();
            RefreshEvents();
        }

        private void LoadCategories()
        {
            _cmbCategory.Items.Add("All categories");
            foreach (var category in EventRepository.GetCategories())
            {
                _cmbCategory.Items.Add(category);
            }
            _cmbCategory.SelectedIndex = 0;
        }

        private void RefreshEvents()
        {
            string selectedCategory = _cmbCategory.SelectedItem == null
                ? "All categories"
                : _cmbCategory.SelectedItem.ToString();
            var events = EventRepository.Search(_txtSearch.Text, selectedCategory).ToList();

            _lvEvents.BeginUpdate();
            _lvEvents.Items.Clear();
            foreach (var localEvent in events)
            {
                var item = new ListViewItem(localEvent.DateDisplay);
                item.SubItems.Add(localEvent.Category);
                item.SubItems.Add(localEvent.Title);
                item.SubItems.Add(localEvent.Venue);
                item.Tag = localEvent;
                _lvEvents.Items.Add(item);
            }
            _lvEvents.EndUpdate();

            _lblCount.Text = events.Count + " item" + (events.Count == 1 ? string.Empty : "s") + " found";
            if (events.Count > 0)
            {
                _lvEvents.Items[0].Selected = true;
                _lvEvents.Items[0].Focused = true;
            }
            else
            {
                _rtbDetails.Text = "No events or announcements match the current filters.";
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            RefreshEvents();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _txtSearch.Clear();
            _cmbCategory.SelectedIndex = 0;
            RefreshEvents();
        }

        private void LvEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lvEvents.SelectedItems.Count == 0) return;
            var selected = _lvEvents.SelectedItems[0].Tag as LocalEvent;
            if (selected == null) return;

            _rtbDetails.Text =
                selected.Title + Environment.NewLine +
                new string('=', selected.Title.Length) + Environment.NewLine + Environment.NewLine +
                "Type: " + selected.Category + Environment.NewLine +
                "Date: " + selected.DateDisplay + Environment.NewLine +
                "Venue / area: " + selected.Venue + Environment.NewLine +
                "Organiser: " + selected.Organizer + Environment.NewLine + Environment.NewLine +
                selected.Description;
        }
    }
}
