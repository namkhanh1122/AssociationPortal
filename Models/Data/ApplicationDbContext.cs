using Microsoft.EntityFrameworkCore;
using AssociationPortal.Models;

namespace AssociationPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Member> Members { get; set; }
    }
}
