using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssociationPortal.Models
{
    [Table("post")]
    public class Post
    {
        [Key]
        [Column("post_id")]
        public long PostId { get; set; }

        [Column("member_id")]
        public long MemberId { get; set; }

        [Column("category_id")]
        public long? CategoryId { get; set; } 

        [Column("post_title")]
        public string PostTitle { get; set; }

        [Column("post_content")]
        public string PostContent { get; set; }

        [Column("post_status")]
        public int? PostStatus { get; set; }

        [Column("post_thumbnail_url")]
        public string? PostThumbnailUrl { get; set; }

        [Column("view_count")]
        public int? ViewCount { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("update_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("approved_by")]
        public long? ApprovedBy { get; set; }

        [Column("approved_status")]
        public int? ApprovedStatus { get; set; } // 0 pending, 1 approved, 2 rejected

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        [Column("rejected_reason")]
        public string? RejectedReason { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [ForeignKey("MemberId")]
        public Member? Member { get; set; }
    }
}