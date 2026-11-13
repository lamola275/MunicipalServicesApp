using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// In-memory source of local events and municipal announcements.
    /// </summary>
    public static class EventRepository
    {
        private static readonly List<LocalEvent> _events = new List<LocalEvent>();

        static EventRepository()
        {
            SeedEvents();
        }

        public static IReadOnlyList<LocalEvent> GetAll()
        {
            return _events.OrderBy(e => e.StartDate).ToList().AsReadOnly();
        }

        public static IReadOnlyList<string> GetCategories()
        {
            return _events.Select(e => e.Category).Distinct().OrderBy(c => c).ToList().AsReadOnly();
        }

        public static IEnumerable<LocalEvent> Search(string text, string category)
        {
            IEnumerable<LocalEvent> query = GetAll();
            if (!string.IsNullOrWhiteSpace(category) && category != "All categories")
            {
                query = query.Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(text))
            {
                string needle = text.Trim();
                query = query.Where(e =>
                    (e.Title ?? string.Empty).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (e.Description ?? string.Empty).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (e.Venue ?? string.Empty).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return query.OrderBy(e => e.StartDate).ToList();
        }

        private static void SeedEvents()
        {
            DateTime today = DateTime.Today;
            _events.Add(new LocalEvent
            {
                Title = "Community Clean-Up: Jukskei River",
                Category = "Environment",
                Venue = "Alexandra, Johannesburg",
                Description = "Volunteers are invited to join the monthly river clean-up. Gloves and refuse bags will be provided.",
                StartDate = today.AddDays(3).AddHours(8),
                EndDate = today.AddDays(3).AddHours(12),
                Organizer = "Waste and Environmental Services"
            });
            _events.Add(new LocalEvent
            {
                Title = "Public Meeting: Budget and IDP Feedback",
                Category = "Community Meeting",
                Venue = "Cape Town Civic Centre",
                Description = "Residents can comment on the draft municipal budget and Integrated Development Plan priorities.",
                StartDate = today.AddDays(5).AddHours(18),
                EndDate = today.AddDays(5).AddHours(20),
                Organizer = "Office of the Mayor"
            });
            _events.Add(new LocalEvent
            {
                Title = "Planned Water Maintenance",
                Category = "Announcement",
                Venue = "Umhlanga and surrounding areas",
                Description = "Water supply may be interrupted between 09:00 and 15:00 while valves are replaced.",
                StartDate = today.AddDays(1).AddHours(9),
                EndDate = today.AddDays(1).AddHours(15),
                Organizer = "Water and Sanitation",
                IsAnnouncement = true
            });
            _events.Add(new LocalEvent
            {
                Title = "Youth Skills and Careers Expo",
                Category = "Education",
                Venue = "Durban ICC",
                Description = "Career guidance, learnership information, CV support and exhibitor stalls for young residents.",
                StartDate = today.AddDays(9).AddHours(9),
                EndDate = today.AddDays(10).AddHours(16),
                Organizer = "Economic Development Unit"
            });
            _events.Add(new LocalEvent
            {
                Title = "Road Resurfacing Notice",
                Category = "Announcement",
                Venue = "Nelson Mandela Drive, Bloemfontein",
                Description = "Lane closures will be in place overnight while resurfacing work is completed.",
                StartDate = today.AddDays(2).AddHours(19),
                EndDate = today.AddDays(4).AddHours(5),
                Organizer = "Roads and Stormwater",
                IsAnnouncement = true
            });
            _events.Add(new LocalEvent
            {
                Title = "Heritage Market Day",
                Category = "Arts and Culture",
                Venue = "Green Point Park, Cape Town",
                Description = "Local food, craft stalls, live music and family activities celebrating South African heritage.",
                StartDate = today.AddDays(14).AddHours(10),
                EndDate = today.AddDays(14).AddHours(17),
                Organizer = "Parks and Recreation"
            });
            _events.Add(new LocalEvent
            {
                Title = "Neighbourhood Safety Forum",
                Category = "Public Safety",
                Venue = "Mitchells Plain Community Hall",
                Description = "Monthly meeting with municipal law enforcement, SAPS and neighbourhood watch representatives.",
                StartDate = today.AddDays(7).AddHours(18),
                EndDate = today.AddDays(7).AddHours(20),
                Organizer = "Community Safety"
            });
            _events.Add(new LocalEvent
            {
                Title = "Free Basic Services Registration Drive",
                Category = "Community Support",
                Venue = "Soweto Customer Service Centre",
                Description = "Qualifying households can receive help registering for free basic water, electricity and sanitation.",
                StartDate = today.AddDays(4).AddHours(8).AddMinutes(30),
                EndDate = today.AddDays(4).AddHours(15),
                Organizer = "Customer Relations"
            });
        }
    }
}
