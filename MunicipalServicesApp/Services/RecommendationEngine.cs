using System;
using System.Collections.Generic;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Services
{

    public static class RecommendationEngine
    {
        private const int MaxRecentSearches = 10;

        private static readonly Dictionary<string, int> _categoryHits =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, int> _termHits =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static readonly Queue<string> _recentSearches = new Queue<string>();

        private static readonly Stack<LocalEvent> _recentlyViewed = new Stack<LocalEvent>();

        public static HashSet<string> UniqueSearchTerms
        {
            get { return new HashSet<string>(_termHits.Keys, StringComparer.OrdinalIgnoreCase); }
        }

        public static int SearchCount
        {
            get { return _recentSearches.Count; }
        }

        public static List<LocalEvent> RecentlyViewed
        {
            get { return new List<LocalEvent>(_recentlyViewed); }
        }

        public static void RecordSearch(string keyword, string category)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                if (!_categoryHits.ContainsKey(category)) _categoryHits[category] = 0;
                _categoryHits[category]++;
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                _recentSearches.Enqueue(keyword.Trim());
                while (_recentSearches.Count > MaxRecentSearches)
                {
                    _recentSearches.Dequeue();
                }

                foreach (string term in SplitTerms(keyword))
                {
                    if (!_termHits.ContainsKey(term)) _termHits[term] = 0;
                    _termHits[term]++;
                }
            }
        }

        public static void RecordView(LocalEvent ev)
        {
            if (ev == null) return;
            _recentlyViewed.Push(ev);
        }

        public static List<LocalEvent> Recommend(IEnumerable<LocalEvent> candidates, int max)
        {
            var scored = new List<KeyValuePair<int, LocalEvent>>();

            var viewedIds = new HashSet<int>();
            foreach (var v in _recentlyViewed) viewedIds.Add(v.Id);

            foreach (var ev in candidates)
            {
                int score = 0;

                int catHits;
                if (_categoryHits.TryGetValue(ev.Category, out catHits))
                {
                    score += catHits * 3;
                }

                foreach (var term in _termHits)
                {
                    if (ev.Title.IndexOf(term.Key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        ev.Description.IndexOf(term.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        score += term.Value * 2;
                    }
                }

                if ((ev.Date.Date - DateTime.Today).Days <= 7) score += 1;
                if (viewedIds.Contains(ev.Id)) score -= 5;

                scored.Add(new KeyValuePair<int, LocalEvent>(score, ev));
            }

            scored.Sort(delegate (KeyValuePair<int, LocalEvent> a, KeyValuePair<int, LocalEvent> b)
            {
                int cmp = b.Key.CompareTo(a.Key);
                if (cmp != 0) return cmp;
                return a.Value.Date.CompareTo(b.Value.Date);
            });

            var result = new List<LocalEvent>();
            for (int i = 0; i < scored.Count && i < max; i++)
            {
                result.Add(scored[i].Value);
            }
            return result;
        }

        public static string PreferenceSummary()
        {
            if (_categoryHits.Count == 0 && _termHits.Count == 0)
            {
                return "Search for events and we will learn what you like.";
            }

            string topCategory = null;
            int topCount = -1;
            foreach (var pair in _categoryHits)
            {
                if (pair.Value > topCount)
                {
                    topCount = pair.Value;
                    topCategory = pair.Key;
                }
            }

            if (topCategory != null)
            {
                return "You seem interested in " + topCategory + " events.";
            }
            return "Recommendations are based on your recent searches.";
        }

        private static IEnumerable<string> SplitTerms(string phrase)
        {
            var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in phrase.Split(new[] { ' ', ',', ';', '.' },
                     StringSplitOptions.RemoveEmptyEntries))
            {
                if (raw.Length >= 3)
                {
                    terms.Add(raw.Trim().ToLowerInvariant());
                }
            }
            return terms;
        }
    }
}
