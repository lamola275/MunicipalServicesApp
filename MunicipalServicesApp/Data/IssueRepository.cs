using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// In-memory repository for reported issues. Seeded examples make the status
    /// page immediately demonstrable while new submissions are appended at runtime.
    /// </summary>
    public static class IssueRepository
    {
        private static readonly List<IssueReport> _issues = new List<IssueReport>();
        private static int _nextNumber = 1;

        private static readonly string[] StatusFlow =
        {
            "Submitted",
            "Acknowledged",
            "In Progress",
            "Resolved",
            "Closed"
        };

        static IssueRepository()
        {
            SeedDemoData();
        }

        public static string Add(IssueReport issue)
        {
            issue.ReferenceNumber = "MS-" + _nextNumber.ToString("D5");
            _nextNumber++;

            if (issue.Priority < 1 || issue.Priority > 5)
            {
                issue.Priority = ResolvePriority(issue.Category);
            }
            if (string.IsNullOrWhiteSpace(issue.AssignedDepartment))
            {
                issue.AssignedDepartment = ResolveDepartment(issue.Category);
            }
            if (issue.StatusHistory.Count == 0)
            {
                issue.AddStatusHistory(issue.DateSubmitted, "Submitted", "Report received from the citizen portal.");
            }

            _issues.Add(issue);
            return issue.ReferenceNumber;
        }

        public static IReadOnlyList<IssueReport> GetAll()
        {
            return _issues.OrderBy(i => i.DateSubmitted).ThenBy(i => i.ReferenceNumber).ToList().AsReadOnly();
        }

        public static IssueReport FindByReference(string referenceNumber)
        {
            if (string.IsNullOrWhiteSpace(referenceNumber)) return null;
            return _issues.FirstOrDefault(i =>
                string.Equals(i.ReferenceNumber, referenceNumber.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static bool AdvanceStatus(string referenceNumber)
        {
            var issue = FindByReference(referenceNumber);
            if (issue == null) return false;

            int currentIndex = Array.IndexOf(StatusFlow, issue.Status);
            if (currentIndex < 0 || currentIndex >= StatusFlow.Length - 1) return false;

            string nextStatus = StatusFlow[currentIndex + 1];
            issue.AddStatusHistory(nextStatus, GetStatusNote(nextStatus));
            return true;
        }

        public static IReadOnlyList<string> GetStatuses()
        {
            return Array.AsReadOnly(StatusFlow);
        }

        public static int Count
        {
            get { return _issues.Count; }
        }

        private static string GetStatusNote(string status)
        {
            switch (status)
            {
                case "Acknowledged": return "The municipality has acknowledged and triaged the request.";
                case "In Progress": return "A municipal work crew has started attending to the request.";
                case "Resolved": return "The field work is complete and is awaiting final confirmation.";
                case "Closed": return "The request has been reviewed and formally closed.";
                default: return "Status updated.";
            }
        }

        private static int ResolvePriority(string category)
        {
            switch (category)
            {
                case "Electricity":
                case "Water Supply":
                case "Public Safety":
                    return 1;
                case "Roads and Potholes":
                case "Sanitation":
                    return 2;
                case "Streetlights":
                case "Waste Management":
                    return 3;
                default:
                    return 4;
            }
        }

        private static string ResolveDepartment(string category)
        {
            switch (category)
            {
                case "Electricity":
                case "Streetlights":
                    return "Energy and Electricity";
                case "Water Supply":
                case "Sanitation":
                    return "Water and Sanitation";
                case "Roads and Potholes":
                    return "Roads and Stormwater";
                case "Waste Management":
                    return "Waste and Environmental Services";
                case "Public Safety":
                    return "Community Safety";
                case "Parks and Recreation":
                    return "Parks and Recreation";
                default:
                    return "General Municipal Services";
            }
        }

        private static void SeedDemoData()
        {
            AddSeed("Water Supply", "Corner of Main Road and 5th Avenue, Johannesburg",
                "Burst water pipe causing flooding across the pavement and reducing pressure to nearby homes.",
                "Submitted", 1, 12, 0);
            AddSeed("Roads and Potholes", "Chris Hani Road, Soweto",
                "Large pothole damaging vehicles near the taxi rank. Temporary warning signs have blown over.",
                "Acknowledged", 2, 10, 2);
            AddSeed("Electricity", "KwaMashu Township, Durban",
                "Street block has been without electricity since last night after a transformer fault.",
                "In Progress", 1, 9, 5);
            AddSeed("Sanitation", "Mamelodi East, Pretoria",
                "Blocked sewer line overflowing near a school entrance and creating a health hazard.",
                "In Progress", 2, 8, 7);
            AddSeed("Streetlights", "Long Street, Cape Town CBD",
                "Four consecutive streetlights are not working, leaving the pedestrian walkway dark.",
                "Resolved", 3, 7, 11);
            AddSeed("Waste Management", "Sandton Drive, Sandton",
                "Wheelie bins have not been collected for two scheduled pickup days.",
                "Submitted", 3, 6, 1);
            AddSeed("Public Safety", "Mitchells Plain Town Centre, Cape Town",
                "Broken pedestrian crossing signal creates a risk for school children during peak traffic.",
                "Acknowledged", 1, 5, 3);
            AddSeed("Parks and Recreation", "Green Point Park, Cape Town",
                "Damaged swing set and exposed bolts on the playground equipment.",
                "Closed", 4, 4, 16);
            AddSeed("Water Supply", "Umhlanga Ridge, Durban",
                "Persistent low water pressure affecting several apartment buildings in the morning.",
                "Acknowledged", 2, 3, 4);
            AddSeed("Roads and Potholes", "Nelson Mandela Drive, Bloemfontein",
                "Multiple potholes forming after recent rain along the left-hand lane.",
                "Resolved", 2, 2, 13);
            AddSeed("Electricity", "Polokwane Central",
                "Exposed electrical kiosk wiring after the access panel was damaged.",
                "Submitted", 1, 1, 0);
            AddSeed("Waste Management", "Gqeberha Beachfront",
                "Overflowing public bins near the promenade after the weekend event.",
                "In Progress", 3, 0, 6);
        }

        private static void AddSeed(
            string category,
            string location,
            string description,
            string status,
            int priority,
            int daysAgoSubmitted,
            int daysAgoUpdated)
        {
            var issue = new IssueReport
            {
                Category = category,
                Location = location,
                Description = description,
                Priority = priority,
                AssignedDepartment = ResolveDepartment(category),
                DateSubmitted = DateTime.Now.AddDays(-daysAgoSubmitted).AddHours(-priority),
                LastUpdated = DateTime.Now.AddDays(-daysAgoUpdated)
            };

            issue.AddStatusHistory(issue.DateSubmitted, "Submitted", "Report received from the citizen portal.");
            int targetIndex = Array.IndexOf(StatusFlow, status);
            DateTime historyTime = issue.DateSubmitted;
            for (int i = 1; i <= targetIndex; i++)
            {
                historyTime = historyTime.AddHours(9 + priority);
                if (historyTime > issue.LastUpdated) historyTime = issue.LastUpdated;
                issue.AddStatusHistory(historyTime, StatusFlow[i], GetStatusNote(StatusFlow[i]));
            }
            issue.LastUpdated = DateTime.Now.AddDays(-daysAgoUpdated);
            Add(issue);
        }
    }
}
