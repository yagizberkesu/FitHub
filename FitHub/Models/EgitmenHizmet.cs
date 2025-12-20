namespace FitHub.Models
{
    public class EgitmenHizmet
    {
        public int EgitmenId { get; set; }
        public Egitmen? Egitmen { get; set; }

        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }
    }
}
