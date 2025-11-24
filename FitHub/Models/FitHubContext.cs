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

            // Admin kullanýcýsý seed verisi
            modelBuilder.Entity<Uye>().HasData(new Uye
            {
                Id = 1,
                Ad = "Admin",
                Soyad = "Kullanýcý",
                Email = "ogrencinumarasi@sakarya.edu.tr",
                Sifre = "sau", // Gerçek projede hash kullanýlmalý!
                Telefon = "0000000000",
                DogumTarihi = new DateTime(1990, 1, 1),
                Cinsiyet = "Diðer",
                Rol = "Admin"
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Egitmen> Egitmenler { get; set; }
        public DbSet<Uye> Uyeler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
    }
}
