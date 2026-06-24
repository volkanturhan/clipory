# clipory

**[English](README.md) | Türkçe**

Hafif bir Windows pano geçmişi yöneticisi.

clipory sistem tepsisinde sessizce durur ve kopyaladığın her şeyi hatırlar.
Bir kısayola basıp son kopyaladıklarını açarsın, birini seçersin ve o an
çalıştığın uygulamaya doğrudan yapışır — bir şeyi "üstüne bir şey daha
kopyaladım" diye kaybetmek yok.

<p align="center">
  <img src="docs/screenshot.png" alt="clipory popup" width="360" />
</p>

## Özellikler

- **Pano geçmişi** — en son kopyaladığın metinleri tutar.
- **Hızlı erişim** — global kısayol (`Ctrl + Shift + V`) aranabilir bir liste açar.
- **Anında geri yapıştır** — bir öğe seç, aktif uygulamaya yapışsın.
- **Hızlı temizle** — birden çok kopyayı seç (Ctrl-tık veya Shift) ve tek seferde sil.
- **Favoriler** — sık kullandığın kopyaları sabitle; hep üstte kalır, asla silinmez.
- **Yeniden başlatmaya dayanır** — geçmişin (ve sabitlerin) kaydedilip geri yüklenir.
- **Windows ile başla** — isteğe bağlı, tepsi menüsünden aç/kapa.
- **Kendini günceller** — yeni sürüm çıkınca clipory tepsiden teklif eder; tek tıkla kurulur.
- **İngilizce & Türkçe** — arayüz dilini tepsiden değiştir.
- **Karanlık mod** — tepsiden Sistem / Koyu / Açık tema (varsayılan Windows'u takip eder).
- **Yoldan çekilir** — sistem tepsisinde çalışır, görev çubuğunu meşgul etmez.
- **Tasarımı gereği gizli** — her şey senin makinende kalır, hiçbir şey yüklenmez.

## İndir

En güncel sürümü [**Releases**](https://github.com/volkanturhan/clipory/releases/latest) sayfasından indir:

- **clipory-setup-…exe** — kurulum (önerilen). Yönetici izni gerekmez ve clipory bundan sonra kendini güncel tutar.
- **clipory-…exe** — taşınabilir tek dosya; çalıştır yeter, kurulum yok.

İkisi de self-contained, yani .NET kurulu olması gerekmez. Windows 10/11, 64-bit.

## Kaynaktan çalıştır

Kendin derlemeyi mi tercih edersin? Windows'ta [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
(sadece runtime değil, SDK) kurulu olmalı.

```bash
git clone https://github.com/volkanturhan/clipory.git
cd clipory
dotnet run --project clipory/clipory.csproj
```

clipory sessizce sistem tepsisinde başlar — **hiçbir pencere açılmaz**. Bu
normaldir; kullanmak için kısayola bas ya da tepsi ikonuna tıkla (aşağıdaki
**Nasıl kullanılır**'a bak).

## Nasıl kullanılır

1. clipory'i başlat — sessizce sistem tepsisine yerleşir.
2. Her zamanki gibi kopyala; clipory hatırlar.
3. **`Ctrl + Shift + V`** ile popup'ı, içinde olduğun uygulamanın üstünde aç.
4. Yazarak filtrele, **↑ / ↓** ile gez, **Enter** (veya çift tıkla) ile seçtiğin
   kopyayı o uygulamaya geri yapıştır.
5. Bir kopyaya **sağ tıkla** (veya **Ctrl + P**) sabitle. **Ctrl-tık** veya
   **Shift + ↑/↓** ile birden çoğunu seç, **Del** ile hepsini birden sil.
6. **Esc** veya boşluğa tıklamak popup'ı kapatır.

Tepsi ikonuna sağ tık: **Aç**, **Geçmişi temizle**, **Windows ile başlat**,
**Çıkış**.

## Verilerin nerede tutulur

Geçmiş yerel olarak `%APPDATA%\clipory\history.json` içinde saklanır ve
makinenden asla çıkmaz. Temizlemek için tepsi menüsündeki **Geçmişi temizle**'yi
kullan (sabitlenenler korunur); sabitlenenleri popup'tan tek tek kaldırabilirsin.

## Kendin derle

Yayın dosyalarını yerelde üretmek ister misin? Çıktı repoya dahil edilmez:

```bash
# Taşınabilir self-contained exe + Windows kurulumu, dist/release içine.
# (Kurulum adımı Inno Setup ister: winget install JRSoftware.InnoSetup)
pwsh tools/release.ps1
```

## Teknoloji

- C# / WPF, .NET 8 (Windows)
- Üçüncü parti bağımlılık yok

## Lisans

MIT — bkz. [LICENSE](LICENSE).
