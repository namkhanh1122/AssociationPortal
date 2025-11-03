using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AssociationPortal.Models
{
    [Table("permision")]
    public class Permision
    {
        [Key]
        [Column("permision_id")]
        public int PermisionId { get; set; }

        [Column("permision_name")]
        public string PermisionName { get; set; }
    }
}
