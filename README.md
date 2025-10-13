# AtlasCopcoMT6000

Atlas Copco MT6000 denetleyicisine TCP üzerinden bağlanıp MID mesajlarıyla (Open Protocol) veri alışverişi yapan bir .NET Framework 4.8 konsol uygulaması. Uygulama başlangıç el sıkışmasını (MID0001), PSET listeleme/seçimini (MID0010/MID0018) ve sıkma sonuçlarının dinlenmesini (MID0008) gerçekleştirir. Gelen ham mesajları ayrıştırarak tork, açı, zaman ve durum gibi alanları konsolda gösterir.

## İçerik
- Özellikler
- Mimari ve Akış
- Kurulum
- Çalıştırma
- Konfigürasyon
- Protokol Detayları (MID Akışı)
- Veri Ayrıştırma (MessageParser)
- Sorun Giderme
- Lisans

## Özellikler
- TCP/IP üzerinden MT6000'e bağlanma
- MID0001 ile bağlantı başlatma
- MID0010 ile PSET listesini alma ve seçme, MID0018 ile PSET ayarı
- MID0008 ile sıkma sonuçlarını dinleme
- Ham mesajları alan/uzunluk tanımına göre ayrıştırıp DTO'ya aktarma

## Mimari ve Akış
- `Program.cs`: Bağlantı, MID mesajlarının gönderimi/okunması ve temel akış
- `MessageParser.cs`: Open Protocol alan tanımlarına göre genel amaçlı ayrıştırıcı
- `AtlasCopcoDataDTO.cs`: Konsolda gösterilen anlamlı alanların taşıyıcısı

Yüksek seviye akış:
1. TCP bağlantısı açılır (`10.145.204.55:4545` varsayılan).
2. MID0001 gönderilir ve cevap doğrulanır.
3. Opsiyonel: MID0010 ile PSET listesi çekilir, kullanıcı seçimi alınır, MID0018 ile ayarlanır.
4. MID0008 gönderilir ve akıştan gelen sonuç mesajları okunur.
5. Mesaj içerikleri ayrıştırılır ve `ControllerName`, `Time`, `TighteningId`, `Torque`, `Angle` ve durum alanları yazdırılır.

## Kurulum
Önkoşullar:
- Windows, Visual Studio 2019/2022
- .NET Framework 4.8 Developer Pack

Bağımlılıklar:
- `Newtonsoft.Json (13.0.3)` (repo içindeki `packages/` klasöründe mevcut)

Adımlar:
1. Depoyu klonlayın veya indirin.
2. `AtlasCopcoMT6000.sln` dosyasını Visual Studio ile açın.
3. Çözümü `Restore NuGet Packages` ile geri yükleyin (otomatik de çalışır).
4. Yapılandırmayı `Debug` veya `Release` seçip projeyi derleyin.

## Çalıştırma
1. `AtlasCopcoMT6000` projesini Başlat (F5).
2. Konsolda bağlantı mesajlarını izleyin.
3. PSET listesi geldikten sonra isterseniz bir PSET ID girin (ör. `7`).
4. Sıkma işlemini tetikleyin; uygulama sonuç mesajlarını alıp ayrıştırır ve konsola yansıtır.

> Not: Varsayılan IP/port `Program.cs` içinde tanımlıdır. Kendi cihaz adresinize göre güncelleyin.

## Konfigürasyon
Varsayılan değerler `Program.cs` içinde sabit tanımlıdır:
- IP: `10.145.204.55`
- Port: `4545`
- Başlangıçta `MID0010` çağrılır, `MID0018` isteğe bağlıdır (yorum kaldırarak etkinleştirin).

Prod veya farklı sahalar için bu değerleri yapılandırılabilir hale getirmek isterseniz `App.config` içine `appSettings` ekleyebilir ve `ConfigurationManager` ile okuyabilirsiniz.

## Protokol Detayları (MID Akışı)
- MID0001 (Client -> Controller): El sıkışma/bağlantı doğrulama.
- MID0010 (Client -> Controller): PSET listesini talep eder.
- MID0018 (Client -> Controller): Seçilen PSET'i ayarlar. Başarı cevabı `0005` beklenir.
- MID0008 (Client -> Controller): Sıkma sonuçlarının akışını başlatır; `1202` kodlu cevaplarda sonuç alanları taşınır.

Konsolda `data.Substring(4, 4) == "1202"` kontrolü ile sonuç mesajı saptanır; ardından ayrıştırma yapılır.

## Veri Ayrıştırma (MessageParser)
`MessageParser.ParseMessage(string)` ham mesajı şu sırayla tarar:
- Parametre ID: 5 karakter
- Uzunluk: 3 karakter (değeri, takip eden `Value` alanının uzunluğunu belirtir)
- Veri Tipi: 2 karakter
- Birim: 3 karakter
- Adım No (StepNo): 4 karakter
- Değer: `Length` kadar karakter

Uygulama içindeki örnek eşleştirmeler:
- `30208` → `ControllerName`
- `30241` → `TorqueSts`
- `30242` → `AngleSts`
- `30200` → `TighteningId`
- `30203` → `Time`
- `30237` → `Torque` (ondalık, `InvariantCulture` ile çözülür)
- `30238` → `Angle` (ondalık, `InvariantCulture` ile çözülür)

JSON çıktısı istenirse: `MessageParser.ConvertToJson(parameters)`.

## Sorun Giderme
- Bağlantı kurulamadı: IP/port ve ağ erişimini kontrol edin; firewall/ACL kuralları.
- Cevap alınmıyor: Open Protocol sürümü ve cihaz konfigürasyonunu doğrulayın; MID uzunluk/biçimi.
- PSET seçimi başarısız: MID0018 cevabında `0004` komut hatası dönebilir; PSET ID’yi ve formatını (`D3`) kontrol edin.
- Ayrıştırma hatası: Ham mesaj uzunluğu beklenenden kısa olabilir; `MessageParser` `index` taşmalarını yakalayıp loglar.

## Lisans
Bu örnek proje Atlas Copco Open Protocol ile entegrasyon konseptini göstermeyi amaçlar. Ticari/üretim kullanımı için güvenlik, hata yönetimi ve yapılandırma sertleştirmeleri yapılmalıdır.


