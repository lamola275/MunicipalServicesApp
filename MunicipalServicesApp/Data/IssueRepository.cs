using System.Collections.Generic;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{

    public static class IssueRepository
    {
        private static readonly List<IssueReport> _issues = new List<IssueReport>();

        private static int _nextNumber = 1;

        public static string Add(IssueReport issue)
        {
            issue.ReferenceNumber = "MS-" + _nextNumber.ToString("D5");
            _nextNumber++;
            _issues.Add(issue);
            return issue.ReferenceNumber;
        }

        public static IReadOnlyList<IssueReport> GetAll()
        {
            return _issues.AsReadOnly();
        }

        public static int Count
        {
            get { return _issues.Count; }
        }
    }
}
