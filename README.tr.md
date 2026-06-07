# ClipStack

**[English](README.md) | Türkçe**

Hafif bir Windows pano geçmişi yöneticisi.

ClipStack sistem tepsisinde sessizce durur ve kopyaladığın her şeyi hatırlar.
Bir kısayola basıp son kopyaladıklarını açarsın, birini seçersin ve o an
çalıştığın uygulamaya doğrudan yapışır — bir şeyi "üstüne bir şey daha
kopyaladım" diye kaybetmek yok.

<p align="center">
  <img src="docs/screenshot.png" alt="ClipStack popup" width="360" />
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

## İndir

> **Not:** ClipStack henüz yayınlanmadı — aşağıdaki link, ilk sürüm
> oluşturulduğunda çalışır hale gelecek.

1. [En son sürümden](https://github.com/volkanturhan/ClipStack/releases/latest)
   **`ClipStack.exe`** dosyasını indir.
2. Çalıştır. Kurulum yok, .NET gerekmiyor — kendi içinde her şeyi barındıran tek dosya.
3. İlk açılışta Windows SmartScreen "bilinmeyen yayıncı" uyarısı verebilir:
   **Ek bilgi → Yine de çalıştır**'a tıkla.

## Nasıl kullanılır

1. ClipStack'i başlat — sessizce sistem tepsisine yerleşir.
2. Her zamanki gibi kopyala; ClipStack hatırlar.
3. **`Ctrl + Shift + V`** ile popup'ı, içinde olduğun uygulamanın üstünde aç.
4. Yazarak filtrele, **↑ / ↓** ile gez, **Enter** (veya çift tıkla) ile seçtiğin
   kopyayı o uygulamaya geri yapıştır.
5. Bir kopyaya **sağ tıkla** (veya **Ctrl + P**) sabitle; **Del** ile sil.
6. **Esc** veya boşluğa tıklamak popup'ı kapatır.

Tepsi ikonuna sağ tık: **Open**, **Clear history**, **Start with Windows**,
**Quit**.

## Verilerin nerede tutulur

Geçmiş yerel olarak `%APPDATA%\ClipStack\history.json` içinde saklanır ve
makinenden asla çıkmaz. Temizlemek için tepsi menüsündeki **Clear history**'yi
kullan (sabitlenenler korunur); sabitlenenleri popup'tan tek tek kaldırabilirsin.

## Kaynaktan derleme

```bash
# Çalıştır
dotnet run --project ClipStack/ClipStack.csproj

# Paylaşılabilir tek dosya exe (çıktı: dist/win-x64/ClipStack.exe)
pwsh tools/publish.ps1
```

## Teknoloji

- C# / WPF, .NET 8 (Windows)
- Üçüncü parti bağımlılık yok

## Lisans

MIT — bkz. [LICENSE](LICENSE).
