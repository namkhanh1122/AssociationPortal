using Microsoft.EntityFrameworkCore;
using AssociationPortal.Models;

namespace AssociationPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Member> Members { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Permision> Permisions { get; set; }

        public DbSet<MemberPermision> MemberPermisions { get; set; }

        public DbSet<PermisionDetail> PermisionDetails { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Document> Documents { get; set; }
        
        public DbSet<Event> Events { get; set; }

        public DbSet<PermissionResult> PermissionResults { get; set; }


    }
}
