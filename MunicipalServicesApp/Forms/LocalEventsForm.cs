using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;
using MunicipalServicesApp.Services;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Local Events and Announcements page.
    ///
    /// Features:
    ///   * Chronological event list backed by a sorted dictionary.
    ///   * Search by keyword, category (from a hash set) and date range.
    ///   * Urgent announcements strip fed by a priority queue.
    ///   * "Recommended for you" panel driven by search-pattern analysis.
    ///   * "Recently viewed" panel backed by a stack.
    /// </summary>
    public class LocalEventsForm : Form
    {
        private readonly TextBox _txtSearch;
        private readonly ComboBox _cmbCategory;
        private readonly CheckBox _chkFrom;
        private readonly CheckBox _chkTo;
        private readonly DateTimePicker _dtpFrom;
        private readonly DateTimePicker _dtpTo;
        private readonly ListView _lvEvents;
        private readonly RichTextBox _rtbDetails;
        private readonly ListBox _lstRecommended;
        private readonly ListBox _lstRecent;
        private readonly Label _lblAnnouncement;
        private readonly Label _lblPreference;
        private readonly Label _lblResultCount;

        public LocalEventsForm()
        {
            // ---- Form setup ------------------------------------------------
            Text = "Local Events and Announcements";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(1100, 780);
            MinimumSize = new Size(900, 640);
            WindowState = FormWindowState.Maximized;
            BackColor = UiTheme.Background;
            Font = UiTheme.LabelFont;
            ForeColor = UiTheme.TextDark;

            // ---- Header ----------------------------------------------------
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = UiTheme.Primary
            };
            var lblHeading = new Label
            {
                Text = "Local Events and Announcements",
                Font = UiTheme.TitleFont,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lblHeading);

            // ---- Urgent announcements strip (priority queue) ---------------
            var announcementStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(255, 243, 224), // soft amber
                Padding = new Padding(12, 0, 12, 0)
            };
            _lblAnnouncement = new Label
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                ForeColor = Color.FromArgb(191, 87, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            announcementStrip.Controls.Add(_lblAnnouncement);

            // ---- Search toolbar --------------------------------------------
            var toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 88,
                ColumnCount = 8,
                RowCount = 2,
                Padding = new Padding(16, 10, 16, 6),
                BackColor = UiTheme.Surface
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // keyword
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f)); // category
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20f)); // from chk
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15f)); // from date
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20f)); // to chk
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15f)); // to date
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f)); // search btn
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f)); // clear btn
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));

            var lblKeyword = MakeToolbarLabel("Keyword:");
            _txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearch.KeyDown += delegate (object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { RunSearch(); e.SuppressKeyPress = true; }
            };

            var lblCategory = MakeToolbarLabel("Category:");
            _cmbCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _chkFrom = new CheckBox { Dock = DockStyle.Fill, Text = "" };
            _chkTo = new CheckBox { Dock = DockStyle.Fill, Text = "" };
            _dtpFrom = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Enabled = false,
                Value = DateTime.Today
            };
            _dtpTo = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Enabled = false,
                Value = DateTime.Today.AddMonths(1)
            };
            _chkFrom.CheckedChanged += delegate { _dtpFrom.Enabled = _chkFrom.Checked; };
            _chkTo.CheckedChanged += delegate { _dtpTo.Enabled = _chkTo.Checked; };

            var btnSearch = MakeToolbarButton("Search", UiTheme.Accent, Color.White);
            btnSearch.Click += delegate { RunSearch(); };
            var btnClear = MakeToolbarButton("Clear", UiTheme.Surface, UiTheme.TextMuted);
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(210, 216, 222);
            btnClear.Click += delegate { ClearSearch(); };

            var lblFrom = MakeToolbarLabel("From date (tick):");
            var lblTo = MakeToolbarLabel("To date (tick):");

            toolbar.Controls.Add(lblKeyword, 0, 0);
            toolbar.Controls.Add(_txtSearch, 0, 1);
            toolbar.Controls.Add(lblCategory, 1, 0);
            toolbar.Controls.Add(_cmbCategory, 1, 1);
            toolbar.Controls.Add(lblFrom, 2, 0);
            toolbar.SetColumnSpan(lblFrom, 2);
            toolbar.Controls.Add(_chkFrom, 2, 1);
            toolbar.Controls.Add(_dtpFrom, 3, 1);
            toolbar.Controls.Add(lblTo, 4, 0);
            toolbar.SetColumnSpan(lblTo, 2);
            toolbar.Controls.Add(_chkTo, 4, 1);
            toolbar.Controls.Add(_dtpTo, 5, 1);
            toolbar.Controls.Add(btnSearch, 6, 1);
            toolbar.Controls.Add(btnClear, 7, 1);

            // ---- Bottom bar: result count + back button --------------------
            var bottomBar = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                ColumnCount = 2,
                Padding = new Padding(16, 6, 16, 6),
                BackColor = UiTheme.Surface
            };
            bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f));
            bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));

            _lblResultCount = new Label
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                ForeColor = UiTheme.TextMuted,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var btnBack = MakeToolbarButton("Back to Main Menu", UiTheme.Primary, Color.White);
            btnBack.Click += delegate { Close(); };
            bottomBar.Controls.Add(_lblResultCount, 0, 0);
            bottomBar.Controls.Add(btnBack, 1, 0);

            // ---- Main content: events | recommended | recent ----------------
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(16, 10, 16, 10)
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56f));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24f));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 72f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));

            // Event list (centre-left).
            _lvEvents = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                Font = UiTheme.LabelFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _lvEvents.Columns.Add("Date", 90);
            _lvEvents.Columns.Add("Type", 90);
            _lvEvents.Columns.Add("Title", 240);
            _lvEvents.Columns.Add("Category", 140);
            _lvEvents.Columns.Add("Venue", 180);
            _lvEvents.SelectedIndexChanged += LvEvents_SelectedIndexChanged;
            _lvEvents.Resize += delegate { ResizeEventColumns(); };

            var eventsPanel = WrapWithCaption(_lvEvents, "Upcoming events (chronological order)");

            // Details box under the event list.
            _rtbDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = UiTheme.LabelFont,
                BackColor = UiTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Select an event to see its details."
            };
            var detailsPanel = WrapWithCaption(_rtbDetails, "Details");

            // Recommended panel.
            _lstRecommended = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _lstRecommended.DoubleClick += LstRecommended_DoubleClick;
            _lblPreference = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.TextMuted,
                TextAlign = ContentAlignment.BottomLeft
            };
            var recommendInner = new Panel { Dock = DockStyle.Fill };
            recommendInner.Controls.Add(_lstRecommended);
            recommendInner.Controls.Add(_lblPreference);
            var recommendPanel = WrapWithCaption(recommendInner,
                "Recommended for you (double-click to open)");

            // Recently viewed panel (stack).
            _lstRecent = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.SmallFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            var recentPanel = WrapWithCaption(_lstRecent, "Recently viewed");

            main.Controls.Add(eventsPanel, 0, 0);
            main.Controls.Add(detailsPanel, 0, 1);
            main.Controls.Add(recommendPanel, 1, 0);
            main.SetRowSpan(recommendPanel, 2);
            main.Controls.Add(recentPanel, 2, 0);
            main.SetRowSpan(recentPanel, 2);

            // Docking order matters: fill last, top/bottom bars first.
            Controls.Add(main);
            Controls.Add(bottomBar);
            Controls.Add(toolbar);
            Controls.Add(announcementStrip);
            Controls.Add(header);

            // ---- Initial data ----------------------------------------------
            EventRepository.EnsureSeeded();
            PopulateCategories();
            LoadEvents(EventRepository.GetAllChronological());
            RefreshAnnouncements();
            RefreshRecommendations();
        }

        // =====================================================================
        //  Behaviour
        // =====================================================================

        private void PopulateCategories()
        {
            _cmbCategory.Items.Clear();
            _cmbCategory.Items.Add("All categories");
            foreach (string cat in EventRepository.GetCategories())
            {
                _cmbCategory.Items.Add(cat);
            }
            _cmbCategory.SelectedIndex = 0;
        }

        private void RunSearch()
        {
            string keyword = _txtSearch.Text;
            string category = _cmbCategory.SelectedIndex > 0
                ? _cmbCategory.SelectedItem.ToString()
                : null;
            DateTime? from = _chkFrom.Checked ? (DateTime?)_dtpFrom.Value.Date : null;
            DateTime? to = _chkTo.Checked ? (DateTime?)_dtpTo.Value.Date : null;

            // Feed the recommendation engine with this search pattern.
            RecommendationEngine.RecordSearch(keyword, category);

            LoadEvents(EventRepository.Search(keyword, category, from, to));
            RefreshRecommendations();
        }

        private void ClearSearch()
        {
            _txtSearch.Clear();
            _cmbCategory.SelectedIndex = 0;
            _chkFrom.Checked = false;
            _chkTo.Checked = false;
            LoadEvents(EventRepository.GetAllChronological());
        }

        private void LoadEvents(List<LocalEvent> events)
        {
            _lvEvents.BeginUpdate();
            _lvEvents.Items.Clear();

            bool alternate = false;
            foreach (var ev in events)
            {
                var item = new ListViewItem(ev.Date.ToString("dd MMM yyyy"));
                item.SubItems.Add(ev.IsAnnouncement ? "Announcement" : "Event");
                item.SubItems.Add(ev.Title);
                item.SubItems.Add(ev.Category);
                item.SubItems.Add(ev.Venue);
                item.Tag = ev;

                if (alternate)
                {
                    item.BackColor = Color.FromArgb(240, 247, 245); // subtle striping
                }
                if (ev.IsAnnouncement && ev.Priority <= 2)
                {
                    item.ForeColor = Color.FromArgb(191, 87, 0); // highlight urgent
                }
                alternate = !alternate;

                _lvEvents.Items.Add(item);
            }

            _lvEvents.EndUpdate();
            ResizeEventColumns();

            _lblResultCount.Text = events.Count == 1
                ? "1 event found"
                : events.Count + " events found";
        }

        private void LvEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lvEvents.SelectedItems.Count == 0) return;

            var ev = (LocalEvent)_lvEvents.SelectedItems[0].Tag;
            ShowDetails(ev);

            // Record the view so recommendations improve (stack).
            RecommendationEngine.RecordView(ev);
            RefreshRecentlyViewed();
            RefreshRecommendations();
        }

        private void ShowDetails(LocalEvent ev)
        {
            _rtbDetails.Text =
                ev.Title + "\r\n" +
                ev.Date.ToString("dddd, dd MMMM yyyy") + "  |  " + ev.Category +
                "  |  " + ev.Venue + "\r\n\r\n" +
                ev.Description;
        }

        private void RefreshAnnouncements()
        {
            var urgent = EventRepository.GetUrgentAnnouncements();
            if (urgent.Count == 0)
            {
                _lblAnnouncement.Text = "No announcements at the moment.";
                return;
            }

            // Most urgent announcement sits at the front of the priority queue.
            var top = urgent[0];
            _lblAnnouncement.Text = "URGENT (" + urgent.Count + " announcements): " +
                top.Title + " - " + top.Date.ToString("dd MMM yyyy") +
                " | " + top.Venue;
        }

        private void RefreshRecommendations()
        {
            _lstRecommended.Items.Clear();
            var recs = RecommendationEngine.Recommend(EventRepository.GetAllChronological(), 6);
            foreach (var ev in recs)
            {
                _lstRecommended.Items.Add(ev);
            }
            _lblPreference.Text = RecommendationEngine.PreferenceSummary();
        }

        private void RefreshRecentlyViewed()
        {
            _lstRecent.Items.Clear();
            var recent = RecommendationEngine.RecentlyViewed;
            for (int i = 0; i < recent.Count && i < 8; i++)
            {
                _lstRecent.Items.Add(recent[i]);
            }
        }

        private void LstRecommended_DoubleClick(object sender, EventArgs e)
        {
            var ev = _lstRecommended.SelectedItem as LocalEvent;
            if (ev == null) return;

            // Jump to the event in the main list so the user sees it in context.
            foreach (ListViewItem item in _lvEvents.Items)
            {
                if (ReferenceEquals(item.Tag, ev))
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    _lvEvents.Focus();
                    return;
                }
            }

            // Not in the current (filtered) list - clear the search and show it.
            ClearSearch();
            foreach (ListViewItem item in _lvEvents.Items)
            {
                if (ReferenceEquals(item.Tag, ev))
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    _lvEvents.Focus();
                    return;
                }
            }
        }

        private void ResizeEventColumns()
        {
            if (_lvEvents.Columns.Count < 5) return;
            int fixed_ = 90 + 90 + 140; // Date + Type + Category
            int remaining = Math.Max(200, _lvEvents.ClientSize.Width - fixed_ - 6);
            _lvEvents.Columns[2].Width = remaining * 55 / 100; // Title
            _lvEvents.Columns[4].Width = remaining * 45 / 100; // Venue
        }

        // =====================================================================
        //  Small UI helpers
        // =====================================================================

        private static Label MakeToolbarLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.TextMuted,
                TextAlign = ContentAlignment.BottomLeft
            };
        }

        private static Button MakeToolbarButton(string text, Color back, Color fore)
        {
            var b = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Cursor = Cursors.Hand,
                Margin = new Padding(4, 2, 4, 2)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static Panel WrapWithCaption(Control content, string caption)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(4)
            };
            var lbl = new Label
            {
                Text = caption,
                Dock = DockStyle.Top,
                Height = 26,
                Font = UiTheme.ButtonFont,
                ForeColor = UiTheme.Primary,
                TextAlign = ContentAlignment.BottomLeft
            };
            panel.Controls.Add(content);
            panel.Controls.Add(lbl);
            return panel;
        }
    }
}
