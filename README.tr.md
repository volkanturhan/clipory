# Clipory

**[English](README.md) | Türkçe**

Hafif bir Windows pano geçmişi yöneticisi.

Clipory sistem tepsisinde sessizce durur ve kopyaladığın her şeyi hatırlar.
Bir kısayola basıp son kopyaladıklarını açarsın, birini seçersin ve o an
çalıştığın uygulamaya doğrudan yapışır — bir şeyi "üstüne bir şey daha
kopyaladım" diye kaybetmek yok.

<p align="center">
  <img src="docs/screenshot.png" alt="Clipory popup" width="360" />
</p>

## Özellikler

- **Pano geçmişi** — en son kopyaladığın metinleri tutar.
- **Hızlı erişim** — global kısayol (`Ctrl + Shift + V`) aranabilir bir liste açar.
- **Anında geri yapıştır** — bir öğe seç, aktif uygulamaya yapışsın.
- **Favoriler** — sık kullandığın kopyaları sabitle; hep üstte kalır, asla silinmez.
- **Yeniden başlatmaya dayanır** — geçmişin (ve sabitlerin) kaydedilip geri yüklenir.
- **Windows ile başla** — isteğe bağlı, tepsi menüsünden aç/kapa.
- **İngilizce & Türkçe** — arayüz dilini tepsiden değiştir.
- **Yoldan çekilir** — sistem tepsisinde çalışır, görev çubuğunu meşgul etmez.
- **Tasarımı gereği gizli** — her şey senin makinende kalır, hiçbir şey yüklenmez.

## Çalıştır

Clipory henüz hazır bir indirme olarak yayınlanmadı, bu yüzden şimdilik
kaynaktan çalıştırıyorsun. Windows'ta [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
(sadece runtime değil, SDK) kurulu olmalı.

```bash
git clone https://github.com/volkanturhan/Clipory.git
cd Clipory
dotnet run --project Clipory/Clipory.csproj
```

Clipory sessizce sistem tepsisinde başlar — **hiçbir pencere açılmaz**. Bu
normaldir; kullanmak için kısayola bas ya da tepsi ikonuna tıkla (aşağıdaki
**Nasıl kullanılır**'a bak).

## Nasıl kullanılır

1. Clipory'i başlat — sessizce sistem tepsisine yerleşir.
2. Her zamanki gibi kopyala; Clipory hatırlar.
3. **`Ctrl + Shift + V`** ile popup'ı, içinde olduğun uygulamanın üstünde aç.
4. Yazarak filtrele, **↑ / ↓** ile gez, **Enter** (veya çift tıkla) ile seçtiğin
   kopyayı o uygulamaya geri yapıştır.
5. Bir kopyaya **sağ tıkla** (veya **Ctrl + P**) sabitle; **Del** ile sil.
6. **Esc** veya boşluğa tıklamak popup'ı kapatır.

Tepsi ikonuna sağ tık: **Aç**, **Geçmişi temizle**, **Windows ile başlat**,
**Çıkış**.

## Verilerin nerede tutulur

Geçmiş yerel olarak `%APPDATA%\Clipory\history.json` içinde saklanır ve
makinenden asla çıkmaz. Temizlemek için tepsi menüsündeki **Geçmişi temizle**'yi
kullan (sabitlenenler korunur); sabitlenenleri popup'tan tek tek kaldırabilirsin.

## Paylaşılabilir exe oluştur

SDK olmadan birine verebileceğin bağımsız bir `.exe` mi istiyorsun? Kendin
derle — çıktı repoya dahil edilmez:

```bash
# dist/ içine derler (self-contained Clipory.exe + lite sürüm)
pwsh tools/publish.ps1
```

## Teknoloji

- C# / WPF, .NET 8 (Windows)
- Üçüncü parti bağımlılık yok

## Lisans

MIT — bkz. [LICENSE](LICENSE).
