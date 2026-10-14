using System;

namespace MunicipalServicesApp.Models
{

    public class LocalEvent
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public DateTime Date { get; set; }

        public string Venue { get; set; }

        public string Description { get; set; }


        public bool IsAnnouncement { get; set; }

        public int Priority { get; set; }

        public LocalEvent()
        {
            IsAnnouncement = false;
            Priority = 5;
        }

        public override string ToString()
        {
            return Date.ToString("dd MMM yyyy") + "  |  " + Title + "  |  " + Category;
        }
    }
}
