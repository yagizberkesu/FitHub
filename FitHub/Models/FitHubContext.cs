using Microsoft.EntityFrameworkCore;

namespace FitHub.Models
{
    public class FitHubContext : DbContext
    {
        public FitHubContext(DbContextOptions<FitHubContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // decimal precision
            modelBuilder.Entity<Randevu>()
                .Property(r => r.Ucret)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasPrecision(18, 2);

            // -----------------------------
            // Egitmen - Hizmet (Many-to-Many)
            // -----------------------------
            modelBuilder.Entity<EgitmenHizmet>()
                .HasKey(x => new { x.EgitmenId, x.HizmetId });

            // Egitmen silinince join satýrlarý silinsin (CASCADE)
            modelBuilder.Entity<EgitmenHizmet>()
                .HasOne(x => x.Egitmen)
                .WithMany(e => e.EgitmenHizmetler)
                .HasForeignKey(x => x.EgitmenId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hizmet silinince join satýrlarý otomatik silinmesin (NO ACTION)
            // SQL Server "multiple cascade paths" çözümü
            modelBuilder.Entity<EgitmenHizmet>()
                .HasOne(x => x.Hizmet)
                .WithMany(h => h.EgitmenHizmetler)
                .HasForeignKey(x => x.HizmetId)
                .OnDelete(DeleteBehavior.NoAction);

            // -----------------------------
            // Randevu FK delete davranýþlarý
            // (SQL Server cascade-path hatalarýný tamamen bitirir)
            // -----------------------------
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.HizmetRef)
                .WithMany()
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Salon)
                .WithMany()
                .HasForeignKey(r => r.SalonId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Egitmen)
                .WithMany()
                .HasForeignKey(r => r.EgitmenId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Uye)
                .WithMany()
                .HasForeignKey(r => r.UyeId)
                .OnDelete(DeleteBehavior.NoAction);

            // -----------------------------
            // Admin seed
            // -----------------------------
            modelBuilder.Entity<Uye>().HasData(new Uye
            {
                Id = 1,
                Ad = "Admin",
                Soyad = "Kullanici",
                Email = "ogrencinumarasi@sakarya.edu.tr",
                Sifre = "sau",
                Telefon = "0000000000",
                DogumTarihi = new DateTime(1990, 1, 1),
                Cinsiyet = "Diger",
                Rol = "Admin"
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Egitmen> Egitmenler { get; set; }
        public DbSet<Uye> Uyeler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<EgitmenHizmet> EgitmenHizmetler { get; set; }
    }
}
