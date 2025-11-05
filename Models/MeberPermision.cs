
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AssociationPortal.Models
{
    [Table("member_permision")]
    public class MemberPermision
    {
        [Key]
        [Column("member_permistion_id")]
        public int MemberPermisionId { get; set; }

        [Column("member_id")]
        public long MemberId { get; set; }

        [Column("permision_id")]
        public int PermisionId { get; set; }

        [Column("licensed")]
        public int Licensed { get; set; }

        public virtual Permision Permision { get; set; }
        
    }
}
