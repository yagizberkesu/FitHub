using System.Text;
using System.Text.Json;
using FitHub.Models;

namespace FitHub.Services
{
    public class GroqService : IGroqService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public GroqService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> GenerateCoachPlanAsync(AiCoachViewModel vm)
        {
            var apiKey = _config["Groq:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return "Groq:ApiKey bulunamadı. User Secrets (secrets.json) içine ekle.";

            // Groq OpenAI-compatible endpoint
            var url = "https://api.groq.com/openai/v1/chat/completions";

            // Model: llama-3.1-8b-instant
            var model = _config["Groq:Model"];
            if (string.IsNullOrWhiteSpace(model))
                model = "llama-3.1-8b-instant";

            var system = @"
Sen FitHub uygulamasında çalışan kısa ve net bir fitness koçusun.
Türkçe yaz.
SADECE düz metin yaz (Markdown yok).
Maksimum 1200 karakter.

FORMAT ZORUNLU:
- Tam olarak kullanıcının istediği kadar gün yaz.
- Her gün TAM 5 hareket olacak.
- Çıktı sadece şablon + en altta 3 satır (KARDİYO/KURAL/UYARI) olacak, başka hiçbir şey yok.

KRİTİK EKİPMAN KURALI:
Ekipman=Salon ise SADECE SALON_HAREKETLERI listesinden seç.
Ekipman=Ev ise SADECE EV_HAREKETLERI listesinden seç.
Ekipman=Minimal ise SADECE MINIMAL_HAREKETLERI listesinden seç.

YASAK:
- Hareket adı olarak şu kelimeleri ASLA tek başına yazma: Makine, Bar, Dambıl, Alet, Machine, Barbell, Dumbbell
- Hareket adı listede yoksa yazma.

Eğer kurallara uyamazsan tek satır yaz:
HATA: FORMAT_VEYA_HAREKET_LISTESI_UYUSMUYOR

SALON_HAREKETLERI:
Bench Press
Incline Dumbbell Press
Shoulder Press (Machine)
Lat Pulldown
Seated Cable Row
Chest Fly (Machine)
Leg Press
Squat (Smith Machine)
Romanian Deadlift
Leg Extension
Leg Curl
Calf Raise (Machine)
Biceps Curl (Cable)
Triceps Pushdown (Cable)
Plank

EV_HAREKETLERI:
Push-up
Bodyweight Squat
Reverse Lunge
Glute Bridge
Hip Hinge
Pike Push-up
Mountain Climber
Dead Bug
Side Plank
Burpee

MINIMAL_HAREKETLERI (dambıl/band):
Dumbbell Bench Press
Dumbbell Row
Goblet Squat
Dumbbell RDL
Dumbbell Shoulder Press
Band Row
Band Pulldown
Dumbbell Biceps Curl
Dumbbell Triceps Extension
Plank
";

            var user = $@"
Kullanıcı:
Yaş: {vm.Yas}
Boy: {vm.BoyCm} cm
Kilo: {vm.KiloKg} kg
Hedef: {vm.Hedef}
Seviye: {vm.Seviye}
Haftada Gün: {vm.HaftadaGun}
Süre: {vm.Dakika} dk
Ekipman: {vm.Ekipman}
Not: {vm.Notlar ?? "Yok"}

ÇIKTI ŞEKLİ (AYNEN UYGULA):
GÜN 1:
1) Hareket — SetxTekrar — Dinlenme sn
2) Hareket — SetxTekrar — Dinlenme sn
3) Hareket — SetxTekrar — Dinlenme sn
4) Hareket — SetxTekrar — Dinlenme sn
5) Hareket — SetxTekrar — Dinlenme sn
GÜN 2:
...
GÜN {vm.HaftadaGun}:

Kurallar:
- TAM OLARAK {vm.HaftadaGun} gün üret.
- HER GÜN TAM 5 hareket.
- Büyük hareketlerde 3 set, izolasyonda 2 set.
- Tekrar aralığı 8-12. Plank 30-60 sn olabilir.
- Dinlenme 60-90 sn.
- Süre {vm.Dakika} dk içine sığacak şekilde seç (çok fazla büyük hareket verme).
- Günler tekrar etmeyecek şekilde çeşitlendir (Salon ise Push/Pull/Legs/Upper/Lower mantığıyla).
- En altta sadece şu 3 satır olacak:

KARDİYO: (hedefe uygun 10-20 dk, haftada 2-3 gün)
KURAL: (progressive overload: haftalık küçük artış)
UYARI: (ısınma 5-10 dk + form + sakatlık uyarısı)

Özet yazma. Ek açıklama yazma. Sadece şablon + 3 satır.
";






            var payload = new
            {
                model = model,
                messages = new object[]
                {
                    new { role = "system", content = system },
                    new { role = "user", content = user }
                },
                temperature = 0.4,
                max_tokens = 900
            };

            var http = _httpClientFactory.CreateClient();

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var res = await http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return $"Groq hata: {(int)res.StatusCode} {res.ReasonPhrase}\n{body}";

            using var doc = JsonDocument.Parse(body);

            // OpenAI-compatible: choices[0].message.content
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return string.IsNullOrWhiteSpace(content) ? "AI boş cevap döndü." : content!;
        }
    }
}
