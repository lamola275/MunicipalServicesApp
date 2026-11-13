using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;
using MunicipalServicesApp.Services;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Task 3 Service Request Status screen. Citizens can view all requests,
    /// search by unique reference, track progress, and inspect the advanced
    /// data structures that organise the information.
    /// </summary>
    public class ServiceRequestStatusForm : Form
    {
        private readonly ServiceRequestAnalytics _analytics;
        private readonly TextBox _txtSearch;
        private readonly ComboBox _cmbStatus;
        private readonly ComboBox _cmbSort;
        private readonly ListView _lvRequests;
        private readonly RichTextBox _rtbDetails;
        private readonly RichTextBox _rtbStructures;
        private readonly RichTextBox _rtbGraph;
        private readonly Label _lblSummary;
        private readonly Button _btnAdvance;

        public ServiceRequestStatusForm()
        {
            _analytics = new ServiceRequestAnalytics();

            Text = "Service Request Status";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(1240, 780);
            MinimumSize = new Size(1020, 680);
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
                Text = "Service Request Status",
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
                Padding = new Padding(18)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            toolbar.Controls.Add(CreateToolbarLabel("Search:"));
            _txtSearch = new TextBox
            {
                Width = 250,
                Font = UiTheme.LabelFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearch.TextChanged += FilterChanged;
            toolbar.Controls.Add(_txtSearch);

            toolbar.Controls.Add(CreateToolbarLabel("  Status:"));
            _cmbStatus = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.LabelFont
            };
            _cmbStatus.SelectedIndexChanged += FilterChanged;
            toolbar.Controls.Add(_cmbStatus);

            toolbar.Controls.Add(CreateToolbarLabel("  Sort by:"));
            _cmbSort = new ComboBox
            {
                Width = 165,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.LabelFont
            };
            _cmbSort.Items.AddRange(new object[]
            {
                "Reference (tree order)",
                "Priority (heap order)",
                "Newest first",
                "Oldest first"
            });
            _cmbSort.SelectedIndexChanged += FilterChanged;
            toolbar.Controls.Add(_cmbSort);

            var btnReset = new Button
            {
                Text = "Reset",
                Width = 85,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = UiTheme.Surface,
                ForeColor = UiTheme.TextDark,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(190, 199, 205);
            btnReset.Click += BtnReset_Click;
            toolbar.Controls.Add(btnReset);

            _lvRequests = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false,
                GridLines = true,
                BackColor = Color.White,
                Font = UiTheme.SmallFont
            };
            _lvRequests.Columns.Add("Reference", 90);
            _lvRequests.Columns.Add("Priority", 62);
            _lvRequests.Columns.Add("Status", 100);
            _lvRequests.Columns.Add("Category", 150);
            _lvRequests.Columns.Add("Location", 210);
            _lvRequests.Columns.Add("Submitted", 115);
            _lvRequests.SelectedIndexChanged += LvRequests_SelectedIndexChanged;

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont
            };

            var detailsTab = new TabPage("Request Details");
            var detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));

            _rtbDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UiTheme.LabelFont
            };
            var detailsButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            _btnAdvance = new Button
            {
                Text = "Advance Status",
                Width = 165,
                Height = 36,
                Font = UiTheme.ButtonFont,
                BackColor = UiTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnAdvance.FlatAppearance.BorderSize = 0;
            _btnAdvance.Click += BtnAdvance_Click;
            detailsButtons.Controls.Add(_btnAdvance);
            detailsLayout.Controls.Add(_rtbDetails, 0, 0);
            detailsLayout.Controls.Add(detailsButtons, 0, 1);
            detailsTab.Controls.Add(detailsLayout);

            var structuresTab = new TabPage("Trees and Heap");
            _rtbStructures = CreateReadOnlyConsoleBox();
            structuresTab.Controls.Add(_rtbStructures);

            var graphTab = new TabPage("Graph and MST");
            _rtbGraph = CreateReadOnlyConsoleBox();
            graphTab.Controls.Add(_rtbGraph);

            tabs.TabPages.Add(detailsTab);
            tabs.TabPages.Add(structuresTab);
            tabs.TabPages.Add(graphTab);

            var bottomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
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
            _lblSummary = new Label
            {
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = UiTheme.TextMuted,
                Padding = new Padding(0, 12, 18, 0)
            };
            bottomPanel.Controls.Add(btnBack);
            bottomPanel.Controls.Add(_lblSummary);

            root.Controls.Add(toolbar, 0, 0);
            root.SetColumnSpan(toolbar, 2);
            root.Controls.Add(_lvRequests, 0, 1);
            root.Controls.Add(tabs, 1, 1);
            root.Controls.Add(bottomPanel, 0, 2);
            root.SetColumnSpan(bottomPanel, 2);

            Controls.Add(root);
            Controls.Add(header);

            LoadStatusFilter();
            _cmbSort.SelectedIndex = 0;
            RefreshAll();
        }

        private static Label CreateToolbarLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 8, 0, 0)
            };
        }

        private static RichTextBox CreateReadOnlyConsoleBox()
        {
            return new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 39, 46),
                ForeColor = Color.FromArgb(225, 239, 235),
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 9.25f),
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both
            };
        }

        private void LoadStatusFilter()
        {
            _cmbStatus.Items.Add("All statuses");
            foreach (var status in IssueRepository.GetStatuses())
            {
                _cmbStatus.Items.Add(status);
            }
            _cmbStatus.SelectedIndex = 0;
        }

        private void RefreshAll(string referenceToSelect = null)
        {
            string selectedReference = referenceToSelect;
            if (string.IsNullOrWhiteSpace(selectedReference) && _lvRequests.SelectedItems.Count > 0)
            {
                var selected = _lvRequests.SelectedItems[0].Tag as IssueReport;
                selectedReference = selected == null ? null : selected.ReferenceNumber;
            }

            _analytics.Rebuild(IssueRepository.GetAll());
            PopulateRequestList(GetFilteredRequests(), selectedReference);
            UpdateAnalyticsPanels(selectedReference);
        }

        private List<IssueReport> GetFilteredRequests()
        {
            IEnumerable<IssueReport> query;
            switch (_cmbSort.SelectedIndex)
            {
                case 1:
                    query = _analytics.GetRequestsByPriority();
                    break;
                case 2:
                    query = IssueRepository.GetAll().OrderByDescending(r => r.DateSubmitted);
                    break;
                case 3:
                    query = IssueRepository.GetAll().OrderBy(r => r.DateSubmitted);
                    break;
                default:
                    query = _analytics.GetRequestsByReferenceOrder();
                    break;
            }

            string selectedStatus = _cmbStatus.SelectedItem == null
                ? "All statuses"
                : _cmbStatus.SelectedItem.ToString();
            if (selectedStatus != "All statuses")
            {
                query = query.Where(r => string.Equals(r.Status, selectedStatus, StringComparison.OrdinalIgnoreCase));
            }

            string searchText = (_txtSearch.Text ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(r =>
                    r.ReferenceNumber.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Location ?? string.Empty).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Category ?? string.Empty).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Description ?? string.Empty).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return query.ToList();
        }

        private void PopulateRequestList(IList<IssueReport> requests, string referenceToSelect)
        {
            _lvRequests.BeginUpdate();
            _lvRequests.Items.Clear();
            foreach (var request in requests)
            {
                var item = new ListViewItem(request.ReferenceNumber);
                item.SubItems.Add("P" + request.Priority);
                item.SubItems.Add(request.Status);
                item.SubItems.Add(request.Category);
                item.SubItems.Add(request.Location);
                item.SubItems.Add(request.DateSubmitted.ToString("yyyy-MM-dd HH:mm"));
                item.Tag = request;
                if (request.Status == "Resolved" || request.Status == "Closed")
                {
                    item.ForeColor = Color.FromArgb(46, 125, 50);
                }
                else if (request.Priority == 1)
                {
                    item.ForeColor = Color.FromArgb(183, 28, 28);
                }
                _lvRequests.Items.Add(item);
            }
            _lvRequests.EndUpdate();

            ListViewItem itemToSelect = null;
            if (!string.IsNullOrWhiteSpace(referenceToSelect))
            {
                itemToSelect = _lvRequests.Items.Cast<ListViewItem>().FirstOrDefault(i =>
                    string.Equals(i.Text, referenceToSelect, StringComparison.OrdinalIgnoreCase));
            }
            if (itemToSelect == null && _lvRequests.Items.Count > 0)
            {
                itemToSelect = _lvRequests.Items[0];
            }
            if (itemToSelect != null)
            {
                itemToSelect.Selected = true;
                itemToSelect.Focused = true;
                itemToSelect.EnsureVisible();
            }
            else
            {
                _rtbDetails.Text = "No service requests match the current filters.";
                _btnAdvance.Enabled = false;
            }

            _lblSummary.Text = requests.Count + " request" + (requests.Count == 1 ? string.Empty : "s") +
                               " displayed | " + IssueRepository.Count + " stored";
        }

        private void UpdateAnalyticsPanels(string selectedReference)
        {
            _rtbStructures.Text = _analytics.BuildDataStructureSummary();
            _rtbGraph.Text = _analytics.BuildGraphSummary(selectedReference);
        }

        private void LvRequests_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lvRequests.SelectedItems.Count == 0)
            {
                _btnAdvance.Enabled = false;
                return;
            }

            var request = _lvRequests.SelectedItems[0].Tag as IssueReport;
            if (request == null) return;

            _rtbDetails.Text = BuildDetailsText(request);
            _btnAdvance.Enabled = request.Status != "Closed";
            _rtbGraph.Text = _analytics.BuildGraphSummary(request.ReferenceNumber);
        }

        private string BuildDetailsText(IssueReport request)
        {
            var builder = new StringBuilder();
            builder.AppendLine(request.ReferenceNumber + " - " + request.Category);
            builder.AppendLine(new string('=', request.ReferenceNumber.Length + request.Category.Length + 3));
            builder.AppendLine();
            builder.AppendLine("Status: " + request.Status + " (" + request.ProgressPercentage + "% complete)");
            builder.AppendLine("Priority: P" + request.Priority + (request.Priority == 1 ? " - critical" : string.Empty));
            builder.AppendLine("Department: " + request.AssignedDepartment);
            builder.AppendLine("Location: " + request.Location);
            builder.AppendLine("Submitted: " + request.DateSubmitted.ToString("yyyy-MM-dd HH:mm"));
            builder.AppendLine("Last updated: " + request.LastUpdated.ToString("yyyy-MM-dd HH:mm"));
            builder.AppendLine();
            builder.AppendLine("Description");
            builder.AppendLine("-----------");
            builder.AppendLine(request.Description);
            builder.AppendLine();
            builder.AppendLine("Progress timeline");
            builder.AppendLine("-----------------");
            foreach (var entry in request.StatusHistory.OrderBy(h => h.Timestamp))
            {
                builder.AppendLine(entry.ToString());
            }
            builder.AppendLine();
            builder.AppendLine("Unique identifier lookup diagnostics");
            builder.AppendLine("------------------------------------");
            builder.AppendLine(_analytics.GetLookupDiagnostics(request.ReferenceNumber));
            return builder.ToString();
        }

        private void BtnAdvance_Click(object sender, EventArgs e)
        {
            if (_lvRequests.SelectedItems.Count == 0) return;
            var request = _lvRequests.SelectedItems[0].Tag as IssueReport;
            if (request == null) return;

            if (!IssueRepository.AdvanceStatus(request.ReferenceNumber))
            {
                MessageBox.Show(this,
                    "This request is already closed and cannot move to another status.",
                    "No further status",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            RefreshAll(request.ReferenceNumber);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _txtSearch.Clear();
            _cmbStatus.SelectedIndex = 0;
            _cmbSort.SelectedIndex = 0;
            RefreshAll();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            RefreshAll();
        }
    }
}
