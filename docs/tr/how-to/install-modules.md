# VGameKit Modülleri Nasıl Kurulur

## Genel Bakış

VGameKit dört ayrı assembly-definition modülüne ayrılmıştır. Her modülün kendine özgü paket bağımlılıkları vardır. Bu rehber, Unity 6 projesine nasıl ekleneceğini adım adım açıklar.

**Ön Koşullar:** Unity 6000.3.7f1, OpenUPM CLI veya manuel `manifest.json` düzenleme.

---

## Modüller

| Modül | Assembly | Amaç |
|-------|----------|------|
| `VGameKit.Runtime` | `VGameKit.Runtime.asmdef` | Çekirdek çalışma zamanı: DI kapsamları, menüler, spawner, process flows, loglama |
| `VGameKit.IO.Runtime` | `VGameKit.IO.Runtime.asmdef` | `JSonKit` ile JSON kalıcılığı |
| `VGameKit.GA.Runtime` | `VGameKit.GA.Runtime.asmdef` | GameAnalytics entegrasyonu |
| `VGameKit.GoogleAds.Runtime` | `VGameKit.GoogleAds.Runtime.asmdef` | Google Mobile Ads entegrasyonu |

---

## Adım 1 — Scoped registry ekle

`Packages/manifest.json` dosyasını açın ve OpenUPM scoped registry henüz yoksa ekleyin:

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.cysharp",
        "jp.hadashikick.vcontainer",
        "jillejr.newtonsoft.json-for-unity",
        "com.google",
        "com.gameanalytics"
      ]
    }
  ]
}
```

---

## Adım 2 — Temel bağımlılıkları ekle

Tüm modüller aşağıdaki paketlere bağımlıdır. `dependencies` bloğuna ekleyin:

```json
{
  "dependencies": {
    "jp.hadashikick.vcontainer": "1.17.0",
    "com.cysharp.unitask": "2.5.10",
    "com.cysharp.messagepipe": "1.8.1",
    "com.cysharp.messagepipe.vcontainer": "1.8.1",
    "jillejr.newtonsoft.json-for-unity": "13.0.102"
  }
}
```

---

## Adım 3 — Modüle özgü bağımlılıkları ekle

### VGameKit.GA (GameAnalytics)

```json
"com.gameanalytics.sdk": "7.10.5"
```

Bu paket mevcut olduğunda `VGameKit.GA.Runtime` asmdef'i otomatik olarak `GA_ENABLED` sembolünü tanımlar.

### VGameKit.GoogleAds (Google Mobile Ads)

```json
"com.google.ads.mobile": "10.3.0",
"com.google.external-dependency-manager": "1.2.186"
```

`VGameKit.GoogleAds.Runtime` de `com.gameanalytics.sdk` algılandığında `GA_ENABLED` tanımlar.

---

## Adım 4 — VGameKit modüllerini ekle

Modülleri **Package Manager > + > Add package from Git URL** aracılığıyla kurun veya doğrudan `Packages/manifest.json` dosyasındaki `dependencies` bloğuna ekleyin.

### manifest.json ile (önerilen — yalnızca ihtiyacınız olan modülleri ekleyin)

```json
{
  "dependencies": {
    "com.cngz.vgamekit":            "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit#v0.0.4",
    "com.cngz.vgamekit.io":         "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.IO#v0.0.4",
    "com.cngz.vgamekit.ga":         "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GA#v0.0.4",
    "com.cngz.vgamekit.googleads":  "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit.GoogleAds#v0.0.4"
  }
}
```

- `com.cngz.vgamekit` — her zaman gereklidir (core runtime)
- `com.cngz.vgamekit.io` — JSON kalıcılığı için gereklidir
- `com.cngz.vgamekit.ga` — isteğe bağlı, GameAnalytics SDK gerektirir
- `com.cngz.vgamekit.googleads` — isteğe bağlı, Google Mobile Ads SDK gerektirir

### Package Manager arayüzü ile

1. **Window > Package Manager** açın.
2. **+** → **Add package from Git URL** tıklayın.
3. İhtiyacınız olan her modül için Git URL'sini yapıştırın (yukarıdaki listeden).

Unity paketi doğrudan GitHub'dan indirir. Hiçbir dosyanın elle kopyalanması gerekmez.

---

## Adım 5 — Derlemeyi doğrula

```bash
dotnet build VGameKit.slnx -c Debug
```

Sıfır uyarı beklenmektedir. Eğer derleme `GoogleMobileAds.dll` ile ilgili eksik referans hatası verirse, Google Mobile Ads paketinin External Dependency Manager aracılığıyla doğru çözümlendiğinden emin olun.

---

## Koşullu derleme sembolleri

| Sembol | Kim tanımlar | Etkisi |
|--------|-------------|--------|
| `VGameKIT_GA` | `com.gameanalytics.sdk` varken `VGameKit.Runtime.asmdef` | Çekirdek çalışma zamanında GA kancalarını etkinleştirir |
| `GA_ENABLED` | `com.gameanalytics.sdk` varken `VGameKit.GoogleAds.Runtime.asmdef` | Ads modülünde GA event loglamasını etkinleştirir |
| `GAMEKIT_LOG` | **Player Settings > Scripting Define Symbols** içinde manuel eklenmeli | `GKLog.Log` çağrılarını etkinleştirir; release buildlerden çıkarın |

---

## Ayrıca Bkz.

- [Loglama referansı](../reference/api/logging.md)
- [Konfigürasyon referansı](../reference/api/configuration.md)
- [Lifetime Scopes referansı](../reference/api/lifetime-scopes.md)
