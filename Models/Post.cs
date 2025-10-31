using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssociationPortal.Models{
[Table("member")]    
public class Post
{   
    [Key]
    [Column("post_id")]
    public long PostId { get; set; }

    [Column("member_id")]
    public long MemberId { get; set; }

    [Column("category_id")]
    public long CategoryId { get; set; }

    [Required, MaxLength(255)]
    [Column("post_title")]
    public string PostTitle { get; set; }

    [Column("post_content")]
    public string PostContent { get; set; }

    [Column("post_thumbnail_url")]
    public string PostThumbnailUrl { get; set; }

    [Column("post_status")]
    public int PostStatus { get; set; }

    [Column("view_count")]
    public int ViewCount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("update_at")]
    public DateTime? UpdateAt { get; set; }

    [Column("author_name")]
    public string AuthorName { get; set; }

    
    [Column("category_name")]
    public string CategoryName { get; set; }
}
}