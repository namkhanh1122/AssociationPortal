using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssociationPortal.Models
{
    [Table("document")]
    public class Document
    {
        [Key]
        [Column("document_id")]
        public long DocumentId { get; set; }

        [Required]
        [Column("member_id")]
        public long MemberId { get; set; }

        [Column("category_id")]
        public long? CategoryId { get; set; }  // NULLABLE vì DB cho phép

        [Required]
        [MaxLength(255)]
        [Column("document_title")]
        public string DocumentTitle { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        [Column("file_path")]
        public string FilePath { get; set; } = null!;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("document_status")]
        public int DocumentStatus { get; set; } = 0;  // 0: pending, 1: approved, 2: rejected

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        [Column("approved_by")]
        public long? ApprovedBy { get; set; }

        [MaxLength(255)]
        [Column("rejected_reason")]
        public string? RejectedReason { get; set; }

        // Navigation Properties
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual Member? Approver { get; set; }
    }
}