using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssociationPortal.Models
{
    [Table("permision_detail")]
    public class PermisionDetail
    {
        [Key]
        [Column("permision_detail_id")]
        public int PermisionDetailId { get; set; }

        [Column("permision_id")]
        public int PermisionId { get; set; }

        [Column("action_code")]
        public int ActionCode { get; set; }

        [Column("action_name")]
        public string ActionName { get; set; }

        [Column("check_action")]
        public int CheckAction { get; set; }
    }
}
