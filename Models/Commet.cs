using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AssociationPortal.Models
{
    public class Comment
    {
        public long CommentId { get; set; }
        public long MemberId { get; set; }
        public long PostId { get; set; }
        public string CommentContent { get; set; }
        public int CommentStatus { get; set; }
        public DateTime? CreatedAt { get; set; }

        public Member Member { get; set; }
        public Post Post { get; set; }
    }
}
