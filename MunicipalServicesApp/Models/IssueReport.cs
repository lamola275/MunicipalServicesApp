using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// Represents one status change in the life cycle of a municipal service request.
    /// </summary>
    public class StatusHistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        public StatusHistoryEntry()
        {
            Timestamp = DateTime.Now;
        }

        public StatusHistoryEntry(DateTime timestamp, string status, string notes)
        {
            Timestamp = timestamp;
            Status = status;
            Notes = notes;
        }

        public override string ToString()
        {
            return Timestamp.ToString("yyyy-MM-dd HH:mm") + " - " + Status +
                   (string.IsNullOrWhiteSpace(Notes) ? string.Empty : " - " + Notes);
        }
    }

    /// <summary>
    /// Model for a citizen-reported municipal issue / service request.
    /// </summary>
    public class IssueReport
    {
        public string ReferenceNumber { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string AttachmentPath { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Status { get; set; }

        /// <summary>1 = critical, 5 = low priority.</summary>
        public int Priority { get; set; }

        public string AssignedDepartment { get; set; }
        public System.Collections.Generic.List<StatusHistoryEntry> StatusHistory { get; private set; }

        public IssueReport()
        {
            DateSubmitted = DateTime.Now;
            LastUpdated = DateTime.Now;
            Status = "Submitted";
            Priority = 3;
            AssignedDepartment = "General Municipal Services";
            StatusHistory = new System.Collections.Generic.List<StatusHistoryEntry>();
        }

        public void AddStatusHistory(string status, string notes)
        {
            AddStatusHistory(DateTime.Now, status, notes);
        }

        public void AddStatusHistory(DateTime timestamp, string status, string notes)
        {
            StatusHistory.Add(new StatusHistoryEntry(timestamp, status, notes));
            Status = status;
            LastUpdated = timestamp;
        }

        public int ProgressPercentage
        {
            get
            {
                switch (Status)
                {
                    case "Submitted": return 10;
                    case "Acknowledged": return 30;
                    case "In Progress": return 60;
                    case "Resolved": return 90;
                    case "Closed": return 100;
                    default: return 0;
                }
            }
        }

        public override string ToString()
        {
            return ReferenceNumber + " | " + Category + " | " + Location + " | " + Status;
        }
    }
}
