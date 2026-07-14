using Microsoft.EntityFrameworkCore;
using OWASPDotNetLab.Models;

namespace OWASPDotNetLab.Data
{
    /// <summary>
    /// Entity Framework Core database context for the OWASP Lab application.
    /// Uses an in-memory database for training purposes.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedNever();
            modelBuilder.Entity<Product>().Property(p => p.Id).ValueGeneratedNever();
            modelBuilder.Entity<Comment>().Property(c => c.Id).ValueGeneratedNever();
        }
    }
}