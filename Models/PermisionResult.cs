using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AssociationPortal.Models
{
    [Keyless]
    public class PermissionResult
    {
        public int ActionCode { get; set; }
    }
}
