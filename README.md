# FitHub - Fitness Center Yönetim Sistemi

FitHub, spor salonları için geliştirilmiş; üyelik, randevu ve salon yönetimini tek bir çatı altında toplayan web tabanlı bir otomasyon sistemidir.

## 🚀 Özellikler

**Üyeler İçin:**
* **Randevu Al:** Salon, eğitmen ve hizmet seçerek kolayca randevu oluşturma.
* **Akıllı Filtreleme:** Sadece müsait olan saatleri (30 dk aralıklarla) görüntüleme.
* **AI Koç:** Yapay zeka destekli kişisel antrenman programı oluşturma.
* **Geçmiş:** Eski ve gelecek randevuları takip etme.

**Yöneticiler (Admin) İçin:**
* **Salon Yönetimi:** Şube ekleme, kapasite ve çalışma saatlerini (00/30 dk hassasiyetle) ayarlama.
* **Eğitmen Yönetimi:** Eğitmen profili, uzmanlık alanları ve mesai saatleri yönetimi.
* **Randevu Kontrolü:** Tüm randevuları görme, düzenleme ve iptal etme.

## 🛠️ Kullanılan Teknolojiler
* .NET 8.0 (ASP.NET Core MVC)
* Entity Framework Core (Code First)
* MS SQL Server
* Bootstrap 5 & JavaScript (jQuery)
* Google Gemini AI API

## ⚙️ Kurulum

1. Projeyi bilgisayarınıza indirin.
2. `appsettings.json` veya `secrets.json` dosyasına veritabanı bağlantı cümlenizi girin.
3. Terminalde şu komutu çalıştırarak veritabanını oluşturun:
   `dotnet ef database update`
4. Projeyi başlatın:
   `dotnet run`

## 🔐 Kullanım Bilgisi
Proje ilk açıldığında bir üyelik oluşturun. Yönetim paneline erişmek için veritabanından ilgili kullanıcının rolünü "Admin" olarak güncellemeniz gerekebilir.
