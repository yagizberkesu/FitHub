using Microsoft.EntityFrameworkCore;

namespace FitHub.Models
{
    public class FitHubContext : DbContext
    {
        public FitHubContext(DbContextOptions<FitHubContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Randevu>()
                .Property(r => r.Ucret)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Egitmen> Egitmenler { get; set; }
        public DbSet<Uye> Uyeler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
    }
}
