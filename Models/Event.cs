using System;

namespace AssociationPortal.Models
{
    public class Event
    {
        public long EventId { get; set; }
        public long MemberId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EventThumbnail { get; set; }
        public int EventStatus { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public long? ApprovedBy { get; set; }
        public string RejectedReason { get; set; }

        public Member Member { get; set; }
    }
}
