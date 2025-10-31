using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssociationPortal.Models
{
    [Table("member")] 
    public class Member
    {
        [Key]
        [Column("member_id")]
        public long MemberID { get; set; }

        [Required, MaxLength(255)]
        [Column("full_name")]
        public string FullName { get; set; }

        [Required, MaxLength(100)]
        [Column("email")]
        public string Email { get; set; }

        [MaxLength(10)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Column("pass_word_hash")]
        public string PasswordHash { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
