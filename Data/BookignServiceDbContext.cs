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
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<DailyWorkingHours> WorkingHours { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Business>()
                .OwnsOne(b => b.Address);

            modelBuilder.Entity<Business>()
                .HasMany(b => b.WeeklyWorkingHours)
                .WithOne(wh => wh.Business)
                .HasForeignKey(wh => wh.BusinessId);

            modelBuilder.Entity<Business>()
                .HasMany(b => b.Employees)
                .WithOne(e => e.Business)
                .HasForeignKey(e => e.BusinessId);

            modelBuilder.Entity<Business>()
                .HasMany(b => b.Services)
                .WithOne(s => s.Business)
                .HasForeignKey(s => s.BusinessId);

            modelBuilder.Entity<DailyWorkingHours>()
                .HasKey(wh => wh.Id);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Service)
                .WithMany()
                .HasForeignKey(r => r.ServiceId);
        }
    }
}
