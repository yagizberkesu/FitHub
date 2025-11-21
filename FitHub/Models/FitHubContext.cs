using Microsoft.EntityFrameworkCore;

namespace FitHub.Models
{
    public class FitHubContext : DbContext
    {
        public FitHubContext(DbContextOptions<FitHubContext> options) : base(options)
        {
        }

        public DbSet<Salon> Salonlar { get; set; }
    }
}
