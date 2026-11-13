using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// Represents either a local event or a municipal announcement.
    /// </summary>
    public class LocalEvent
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Organizer { get; set; }
        public bool IsAnnouncement { get; set; }

        public string DateDisplay
        {
            get
            {
                if (StartDate.Date == EndDate.Date)
                {
                    return StartDate.ToString("dd MMM yyyy HH:mm");
                }
                return StartDate.ToString("dd MMM yyyy") + " - " + EndDate.ToString("dd MMM yyyy");
            }
        }

        public override string ToString()
        {
            return DateDisplay + " | " + Category + " | " + Title;
        }
    }
}
