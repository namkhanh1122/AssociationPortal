using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AssociationPortal.Models
{
    [Table("category")]
    public class Category
    {
        [Key]
        [Column("category_id")]
        public long CategoryId { get; set; }

        [Column("member_id")]
        public long? MemberId { get; set; }

        [Column("category_name")]
        public string CategoryName { get; set; }

        [Column("decription")]
        public string? Description { get; set; }

        [Column("category_status")]
        public int? CategoryStatus { get; set; }

        // Navigation property
        public ICollection<Post>? Posts { get; set; }
    }
    public class CategoryOptionDto
    {
        public long? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }


}
