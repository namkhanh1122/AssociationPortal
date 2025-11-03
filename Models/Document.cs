using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AssociationPortal.Models
{
    public class Document
    {
        public long DocumentId { get; set; }
        public long MemberId { get; set; }
        public long CategoryId { get; set; }
        public string DocumentTitle { get; set; }
        public string FilePath { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int DocumentStatus { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public long? ApprovedBy { get; set; }
        public string RejectedReason { get; set; }

        public Member Member { get; set; }
        public Category Category { get; set; }
    }
}
