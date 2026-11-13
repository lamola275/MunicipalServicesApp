using System;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Services
{
    /// <summary>
    /// Comparable key stored inside the BST, AVL tree, and red-black tree.
    /// </summary>
    public class RequestKey : IComparable<RequestKey>
    {
        public string ReferenceNumber { get; private set; }
        public IssueReport Request { get; private set; }

        public RequestKey(string referenceNumber, IssueReport request)
        {
            ReferenceNumber = referenceNumber;
            Request = request;
        }

        public int CompareTo(RequestKey other)
        {
            if (other == null) return 1;
            return string.Compare(ReferenceNumber, other.ReferenceNumber, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return ReferenceNumber;
        }
    }

    /// <summary>
    /// Comparable heap item. Critical (1) and older requests rise to the top.
    /// </summary>
    public class RequestPriorityItem : IComparable<RequestPriorityItem>
    {
        public string ReferenceNumber { get; private set; }
        public int Priority { get; private set; }
        public DateTime DateSubmitted { get; private set; }
        public IssueReport Request { get; private set; }

        public RequestPriorityItem(IssueReport request)
        {
            Request = request;
            ReferenceNumber = request.ReferenceNumber;
            Priority = request.Priority;
            DateSubmitted = request.DateSubmitted;
        }

        public int CompareTo(RequestPriorityItem other)
        {
            if (other == null) return 1;
            int priorityComparison = Priority.CompareTo(other.Priority);
            if (priorityComparison != 0) return priorityComparison;
            int dateComparison = DateSubmitted.CompareTo(other.DateSubmitted);
            if (dateComparison != 0) return dateComparison;
            return string.Compare(ReferenceNumber, other.ReferenceNumber, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return "P" + Priority + " | " + ReferenceNumber + " | " + Request.Category + " | " + Request.Status;
        }
    }
}
