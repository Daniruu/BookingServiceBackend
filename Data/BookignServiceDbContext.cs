using Microsoft.EntityFrameworkCore;
using BookingServiceBackend.Models;

namespace BookingServiceBackend.Data
{
    public class BookignServiceDbContext : DbContext
    {
        public BookignServiceDbContext(DbContextOptions<BookignServiceDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Service> Services { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Business>()
                .OwnsOne(b => b.Address);

            modelBuilder.Entity<Business>()
                .HasMany(b => b.WorkingHours)
                .WithOne()
                .HasForeignKey("BusinessId");

            modelBuilder.Entity<WorkingHours>()
                .HasKey(wh => new { wh.BusinessId, wh.Day });
        }
    }
}
