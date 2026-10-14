using System;
using System.Collections.Generic;
using MunicipalServicesApp.DataStructures;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{

    public static class EventRepository
    {
        private static readonly SortedDictionary<DateTime, List<LocalEvent>> _eventsByDate =
            new SortedDictionary<DateTime, List<LocalEvent>>();

        private static readonly Dictionary<string, List<LocalEvent>> _eventsByCategory =
            new Dictionary<string, List<LocalEvent>>(StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> _categories =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static readonly Queue<LocalEvent> _announcementInbox = new Queue<LocalEvent>();

        private static MinPriorityQueue<LocalEvent> _urgentAnnouncements =
            new MinPriorityQueue<LocalEvent>();

        private static int _nextId = 1;
        private static bool _seeded = false;

        public static void EnsureSeeded()
        {
            if (_seeded) return;
            _seeded = true;

            DateTime today = DateTime.Today;

            Add(new LocalEvent
            {
                Title = "Community Clean-Up Drive",
                Category = "Environment",
                Date = today.AddDays(3),
                Venue = "Centurion Lake Park",
                Description = "Join ward councillors and volunteers for a morning clean-up. " +
                              "Gloves, bags and refreshments provided. Wear comfortable shoes."
            });
            Add(new LocalEvent
            {
                Title = "Planned Water Supply Interruption",
                Category = "Water and Sanitation",
                Date = today.AddDays(1),
                Venue = "Diepsloot Ext 4 - 7",
                Description = "Water supply will be interrupted from 08:00 to 16:00 for " +
                              "scheduled maintenance on the main reservoir line. " +
                              "Water tankers will be stationed at the community hall.",
                IsAnnouncement = true,
                Priority = 1
            });
            Add(new LocalEvent
            {
                Title = "Youth Skills Development Workshop",
                Category = "Community Development",
                Date = today.AddDays(7),
                Venue = "Soshanguve Community Hall",
                Description = "Free CV writing, interview skills and computer literacy workshop " +
                              "for residents aged 18-35. Bring a certified copy of your ID."
            });
            Add(new LocalEvent
            {
                Title = "Load Shedding Schedule Update",
                Category = "Electricity",
                Date = today,
                Venue = "All wards",
                Description = "The updated load shedding schedule for this month has been " +
                              "published. Check your block on the municipal website or the " +
                              "notice boards at customer care centres.",
                IsAnnouncement = true,
                Priority = 2
            });
            Add(new LocalEvent
            {
                Title = "Public Participation Meeting: IDP Budget",
                Category = "Governance",
                Date = today.AddDays(10),
                Venue = "City Hall, Pretoria CBD",
                Description = "Have your say on the Integrated Development Plan and the " +
                              "draft municipal budget for the next financial year."
            });
            Add(new LocalEvent
            {
                Title = "Road Resurfacing Notice: Main Road",
                Category = "Roads and Transport",
                Date = today.AddDays(2),
                Venue = "Main Road, Mamelodi East",
                Description = "Resurfacing work between 09:00 and 15:00 daily for two weeks. " +
                              "Expect lane closures and delays. Alternative route via Tsamaya Avenue.",
                IsAnnouncement = true,
                Priority = 2
            });
            Add(new LocalEvent
            {
                Title = "Heritage Day Celebration",
                Category = "Arts and Culture",
                Date = today.AddDays(14),
                Venue = "Freedom Park",
                Description = "Celebrate Heritage Day with traditional music, dance, a braai " +
                              "competition and a craft market. Entry is free."
            });
            Add(new LocalEvent
            {
                Title = "Free Health Screening Day",
                Category = "Health",
                Date = today.AddDays(5),
                Venue = "Atteridgeville Clinic",
                Description = "Free blood pressure, diabetes, HIV and TB screening. " +
                              "No appointment needed - first come, first served."
            });
            Add(new LocalEvent
            {
                Title = "Tree Planting Ceremony",
                Category = "Environment",
                Date = today.AddDays(12),
                Venue = "Ga-Rankuwa Sports Ground",
                Description = "Arbor month tree planting with local schools. " +
                              "Residents are welcome to sponsor a tree."
            });
            Add(new LocalEvent
            {
                Title = "New Library Opening Hours",
                Category = "Community Development",
                Date = today.AddDays(4),
                Venue = "Hammanskraal Library",
                Description = "The library now opens at 08:00 and closes at 17:00 on weekdays, " +
                              "and 08:00 to 13:00 on Saturdays.",
                IsAnnouncement = true,
                Priority = 4
            });
            Add(new LocalEvent
            {
                Title = "Junior Soccer Tournament",
                Category = "Sports and Recreation",
                Date = today.AddDays(9),
                Venue = "Mabopane Stadium",
                Description = "Under-13 and under-15 soccer tournament. Registration closes " +
                              "three days before the event at the sports office."
            });
            Add(new LocalEvent
            {
                Title = "Electricity Meter Audit",
                Category = "Electricity",
                Date = today.AddDays(6),
                Venue = "Akasia and Chantelle",
                Description = "Municipal teams will audit electricity meters. Officials carry " +
                              "ID cards - please verify before allowing access.",
                IsAnnouncement = true,
                Priority = 3
            });
            Add(new LocalEvent
            {
                Title = "Farmers and Craft Market",
                Category = "Arts and Culture",
                Date = today.AddDays(8),
                Venue = "Irene Village Market Grounds",
                Description = "Local produce, handmade crafts and live music. " +
                              "Stall bookings close one week before the market."
            });
            Add(new LocalEvent
            {
                Title = "Crime Awareness Imbizo",
                Category = "Public Safety",
                Date = today.AddDays(11),
                Venue = "Kagiso Community Centre",
                Description = "SAPS and community policing forum present safety tips and the " +
                              "new sector policing contact numbers."
            });
            Add(new LocalEvent
            {
                Title = "Refuse Collection Schedule Change",
                Category = "Waste Management",
                Date = today.AddDays(3),
                Venue = "All wards",
                Description = "Due to the public holiday, refuse collection moves one day " +
                              "later this week. Please put bins out by 07:00.",
                IsAnnouncement = true,
                Priority = 2
            });
            Add(new LocalEvent
            {
                Title = "Marathon: City Fun Run",
                Category = "Sports and Recreation",
                Date = today.AddDays(21),
                Venue = "Starts at Church Square",
                Description = "5 km fun run and 21 km half marathon through the city. " +
                              "Road closures apply from 05:00 to 12:00."
            });
            Add(new LocalEvent
            {
                Title = "Rates Rebate Application Deadline",
                Category = "Finance",
                Date = today.AddDays(15),
                Venue = "Customer Care Centres",
                Description = "Pensioners and indigent households must submit rates rebate " +
                              "applications before the deadline. Bring ID and proof of income.",
                IsAnnouncement = true,
                Priority = 3
            });
            Add(new LocalEvent
            {
                Title = "Mobile Clinic Visit",
                Category = "Health",
                Date = today.AddDays(13),
                Venue = "Winterveld (Next to taxi rank)",
                Description = "The mobile clinic offers immunisations, family planning and " +
                              "chronic medication collection."
            });
        }

        public static void Add(LocalEvent ev)
        {
            ev.Id = _nextId++;

            if (!_eventsByDate.ContainsKey(ev.Date.Date))
            {
                _eventsByDate.Add(ev.Date.Date, new List<LocalEvent>());
            }
            _eventsByDate[ev.Date.Date].Add(ev);

            if (!_eventsByCategory.ContainsKey(ev.Category))
            {
                _eventsByCategory.Add(ev.Category, new List<LocalEvent>());
            }
            _eventsByCategory[ev.Category].Add(ev);

            _categories.Add(ev.Category);

            if (ev.IsAnnouncement)
            {
                _announcementInbox.Enqueue(ev);
                RebuildAnnouncementQueue();
            }
        }

        private static void RebuildAnnouncementQueue()
        {
            var all = _urgentAnnouncements.OrderedItems();
            while (_announcementInbox.Count > 0)
            {
                all.Add(_announcementInbox.Dequeue());
            }

            var rebuilt = new MinPriorityQueue<LocalEvent>();
            foreach (var item in all)
            {
                rebuilt.Enqueue(item, item.Priority);
            }

            _urgentAnnouncements = rebuilt;
        }
        public static List<string> GetCategories()
        {
            EnsureSeeded();
            var list = new List<string>(_categories);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
        }

        public static List<LocalEvent> GetAllChronological()
        {
            EnsureSeeded();
            var result = new List<LocalEvent>();
            foreach (var pair in _eventsByDate)
            {
                result.AddRange(pair.Value);
            }
            return result;
        }

        public static List<LocalEvent> Search(string keyword, string category,
                                              DateTime? from, DateTime? to)
        {
            EnsureSeeded();
            var result = new List<LocalEvent>();
            string needle = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();

            IEnumerable<LocalEvent> pool;
            if (!string.IsNullOrEmpty(category) &&
                _eventsByCategory.ContainsKey(category) &&
                from == null && to == null && needle == null)
            {
                pool = _eventsByCategory[category];
            }
            else
            {
                var flat = new List<LocalEvent>();
                foreach (var pair in _eventsByDate)
                {
                    if (from.HasValue && pair.Key < from.Value.Date) continue;
                    if (to.HasValue && pair.Key > to.Value.Date) continue;
                    flat.AddRange(pair.Value);
                }
                pool = flat;
            }

            var seen = new HashSet<int>(); 
            foreach (var ev in pool)
            {
                if (!seen.Add(ev.Id)) continue;
                if (!string.IsNullOrEmpty(category) &&
                    !string.Equals(ev.Category, category, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (from.HasValue && ev.Date.Date < from.Value.Date) continue;
                if (to.HasValue && ev.Date.Date > to.Value.Date) continue;
                if (needle != null &&
                    ev.Title.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0 &&
                    ev.Description.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0 &&
                    ev.Venue.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }
                result.Add(ev);
            }
            result.Sort(delegate (LocalEvent a, LocalEvent b) { return a.Date.CompareTo(b.Date); });
            return result;
        }

        public static List<LocalEvent> GetUrgentAnnouncements()
        {
            EnsureSeeded();
            return _urgentAnnouncements.OrderedItems();
        }
    }
}
