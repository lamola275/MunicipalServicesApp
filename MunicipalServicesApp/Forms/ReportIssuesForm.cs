using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{

    public class ReportIssuesForm : Form
    {
        private readonly TextBox _txtLocation;
        private readonly ComboBox _cmbCategory;
        private readonly RichTextBox _rtbDescription;
        private readonly Button _btnAttach;
        private readonly Label _lblAttachment;
        private readonly Button _btnSubmit;
        private readonly Button _btnBack;
        private readonly ProgressBar _engagementBar;
        private readonly Label _lblEngagement;

        private string _attachmentPath;

        public ReportIssuesForm()
        {

            Text = "Report an Issue";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(900, 760);
            MinimumSize = new Size(680, 620);
            WindowState = FormWindowState.Maximized; 
            BackColor = UiTheme.Background;
            Font = UiTheme.LabelFont;
            ForeColor = UiTheme.TextDark;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = UiTheme.Primary
            };
            var lblHeading = new Label
            {
                Text = "Report an Issue",
                Font = UiTheme.TitleFont,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lblHeading);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(24, 20, 24, 16)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));  
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));   
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));   
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));  
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));   
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));  
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));   
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));    

  
            var lblLocation = CreateFieldLabel("Location:");
            _txtLocation = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtLocation.TextChanged += Input_Changed;

            var lblCategory = CreateFieldLabel("Category:");
            _cmbCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbCategory.Items.AddRange(new object[]
            {
                "Sanitation",
                "Roads and Potholes",
                "Water Supply",
                "Electricity",
                "Waste Management",
                "Public Safety",
                "Parks and Recreation",
                "Streetlights",
                "Other"
            });
            _cmbCategory.SelectedIndexChanged += Input_Changed;

            var lblDescription = CreateFieldLabel("Description:");
            _rtbDescription = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _rtbDescription.TextChanged += Input_Changed;

            var lblAttachmentCaption = CreateFieldLabel("Attachment:");
            var attachmentPanel = new Panel { Dock = DockStyle.Fill };
            _btnAttach = new Button
            {
                Text = "Attach Image / Document...",
                Width = 210,
                Dock = DockStyle.Left,
                Font = UiTheme.LabelFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = UiTheme.Surface,
                ForeColor = UiTheme.TextDark,
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            _btnAttach.FlatAppearance.BorderColor = Color.FromArgb(189, 199, 205);
            _btnAttach.Click += BtnAttach_Click;

            _lblAttachment = new Label
            {
                Text = "No file attached (optional)",
                Dock = DockStyle.Fill,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.TextMuted,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                AutoEllipsis = true
            };
            attachmentPanel.Controls.Add(_lblAttachment);
            attachmentPanel.Controls.Add(_btnAttach);

            _engagementBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };
            _lblEngagement = new Label
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Accent,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Every report helps build a better community - let's begin!"
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            _btnSubmit = new Button
            {
                Text = "Submit Report",
                Width = 170,
                Height = 40,
                Font = UiTheme.ButtonFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = UiTheme.Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnSubmit.FlatAppearance.BorderSize = 0;
            _btnSubmit.Click += BtnSubmit_Click;

            _btnBack = new Button
            {
                Text = "Back to Main Menu",
                Width = 170,
                Height = 40,
                Font = UiTheme.ButtonFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(224, 229, 234),
                ForeColor = UiTheme.TextDark,
                Cursor = Cursors.Hand
            };
            _btnBack.FlatAppearance.BorderSize = 0;
            _btnBack.Click += delegate { Close(); };
            buttonPanel.Controls.Add(_btnSubmit);
            buttonPanel.Controls.Add(_btnBack);

            layout.Controls.Add(lblLocation, 0, 0);
            layout.Controls.Add(_txtLocation, 1, 0);
            layout.Controls.Add(lblCategory, 0, 1);
            layout.Controls.Add(_cmbCategory, 1, 1);
            layout.Controls.Add(lblDescription, 0, 2);
            layout.Controls.Add(_rtbDescription, 1, 2);
            layout.Controls.Add(lblAttachmentCaption, 0, 3);
            layout.Controls.Add(attachmentPanel, 1, 3);
            layout.Controls.Add(_engagementBar, 1, 4);
            layout.SetColumnSpan(_lblEngagement, 2);
            layout.Controls.Add(_lblEngagement, 0, 5);
            layout.SetColumnSpan(buttonPanel, 2);
            layout.Controls.Add(buttonPanel, 0, 6);

            Controls.Add(layout);
            Controls.Add(header);

            AcceptButton = _btnSubmit;
        }

        private static Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = UiTheme.LabelFont,
                ForeColor = UiTheme.TextDark,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }


        private void Input_Changed(object sender, EventArgs e)
        {
            UpdateEngagement();
        }

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Attach an image or document related to the issue";
                dialog.Filter =
                    "Images and Documents|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.pdf;*.doc;*.docx;*.txt|" +
                    "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif|" +
                    "Documents|*.pdf;*.doc;*.docx;*.txt|" +
                    "All files|*.*";
                dialog.FilterIndex = 1;
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _attachmentPath = dialog.FileName;
                    _lblAttachment.Text = Path.GetFileName(_attachmentPath);
                    _lblAttachment.ForeColor = UiTheme.Accent;
                    UpdateEngagement();
                }
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtLocation.Text))
            {
                ShowValidationError("Please enter the location of the issue.", _txtLocation);
                return;
            }
            if (_cmbCategory.SelectedIndex < 0)
            {
                ShowValidationError("Please select a category for the issue.", _cmbCategory);
                return;
            }
            if (string.IsNullOrWhiteSpace(_rtbDescription.Text) ||
                _rtbDescription.Text.Trim().Length < 10)
            {
                ShowValidationError(
                    "Please provide a description of at least 10 characters so the municipality can understand the issue.",
                    _rtbDescription);
                return;
            }

            var issue = new IssueReport
            {
                Location = _txtLocation.Text.Trim(),
                Category = _cmbCategory.SelectedItem.ToString(),
                Description = _rtbDescription.Text.Trim(),
                AttachmentPath = _attachmentPath
            };
            string referenceNumber = IssueRepository.Add(issue);

            MessageBox.Show(this,
                "Thank you! Your report has been submitted successfully.\n\n" +
                "Reference number: " + referenceNumber + "\n\n" +
                "Keep this number to track your request once the " +
                "'Service Request Status' feature launches.",
                "Report Submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ResetForm();
        }

        private void UpdateEngagement()
        {
            int progress = 0;
            if (!string.IsNullOrWhiteSpace(_txtLocation.Text)) progress += 25;
            if (_cmbCategory.SelectedIndex >= 0) progress += 25;
            if (_rtbDescription.Text.Trim().Length >= 10) progress += 30;
            if (_attachmentPath != null) progress += 20;

            _engagementBar.Value = Math.Min(progress, 100);

            if (progress == 0)
                _lblEngagement.Text = "Every report helps build a better community - let's begin!";
            else if (progress <= 25)
                _lblEngagement.Text = "Great start! Your municipality is listening.";
            else if (progress <= 50)
                _lblEngagement.Text = "Halfway there - keep going!";
            else if (progress < 100)
                _lblEngagement.Text = "Almost done! Your report will make a difference.";
            else
                _lblEngagement.Text = "Excellent! Click 'Submit Report' to send it to your municipality.";
        }


        private void ShowValidationError(string message, Control field)
        {
            MessageBox.Show(this, message, "Incomplete Report",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            field.Focus();
        }

        private void ResetForm()
        {
            _txtLocation.Clear();
            _cmbCategory.SelectedIndex = -1;
            _rtbDescription.Clear();
            _attachmentPath = null;
            _lblAttachment.Text = "No file attached (optional)";
            _lblAttachment.ForeColor = UiTheme.TextMuted;
            UpdateEngagement();
            _txtLocation.Focus();
        }
    }
}
