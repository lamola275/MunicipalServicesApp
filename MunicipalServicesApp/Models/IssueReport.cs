using System;

namespace MunicipalServicesApp.Models
{

    public class IssueReport
    {
        public string ReferenceNumber { get; set; }

        public string Location { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }

        public string AttachmentPath { get; set; }

        public DateTime DateSubmitted { get; set; }

        public string Status { get; set; }

        public IssueReport()
        {
            DateSubmitted = DateTime.Now;
            Status = "Submitted";
        }

        public override string ToString()
        {
            return ReferenceNumber + " | " + Category + " | " + Location + " | " + Status;
        }
    }
}
