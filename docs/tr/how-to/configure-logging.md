# Loglama Nasıl Yapılandırılır

## Genel Bakış

VGameKit'in loglama sistemi `GKConfig` (`ScriptableObject`) ve `GKLog` (statik yardımcı) tarafından kontrol edilir. Log çıktısı, `GAMEKIT_LOG` scripting define sembolü mevcut değilse derleme zamanında tamamen kaldırılır. Bu rehber; config asset'inin oluşturulmasını, log seviyelerinin seçilmesini ve kod içine log çağrılarının nasıl ekleneceğini kapsar.

**Loglama tamamen isteğe bağlıdır.** `GAMEKIT_LOG` sembolünü eklemezseniz veya `LogState`'i `None` olarak bırakırsanız framework hatasız çalışmaya devam eder; herhangi bir log çıktısı üretilmez ve performans üzerinde hiçbir etkisi olmaz.

**Ön Koşullar:** `VGameKit.Runtime` modülü kurulu.

---

## Adım 1 — GKConfig asset'ini oluştur

Unity Editor'da şu yola gidin:

**Assets > Create > VGameKit > Config > GKConfig**

Bu işlem `Assets/Resources/GKConfig.asset` dosyasını oluşturur ve otomatik olarak **Player Settings > Preloaded Assets** listesine ekler. Çalışma zamanında yalnızca bir `GKConfig` asset'i aktif olur.

---

## Adım 2 — Log durumunu ayarla

Project penceresinde `GKConfig` asset'ini seçin ve Inspector'da **Log State** alanını ayarlayın.

`LogState`, `[Flags]` enum'udur — birden fazla kategoriyi aynı anda etkinleştirmek için bitwise OR operatörüyle birleştirin.

| Bayrak | Kategori | Unity konsol rengi |
|--------|----------|--------------------|
| `None` | Loglama devre dışı | — |
| `Core` | Framework dahilileri | varsayılan |
| `Development` | Ayrıntılı debug çıktısı | varsayılan |
| `Info` | Genel bilgi mesajları | varsayılan |
| `Analytics` | Analitik olayları | magenta |
| `IAP` | Uygulama içi satın alma olayları | magenta |
| `Ads` | Reklam yaşam döngüsü olayları | magenta |
| `Warning` | Kritik olmayan uyarılar | sarı |
| `Game` | Kullanıcıya yönelik oyun olayları | yeşil |
| `Pause` | Duraklat/devam et geçişleri | mavi |
| `ProcessFlow` | Process flow yürütme izleme | sarı |
| `Error` | Çalışma zamanı hataları | kırmızı |
| `Fatal` | Kurtarılamaz hatalar | kırmızı |
| `Booster` | Booster/güç olayları | magenta |
| `Timer` | Zamanlayıcı olayları | magenta |

**Önerilen geliştirme yapılandırması:** `Development | Game | ProcessFlow | Error | Warning`

**Önerilen release yapılandırması:** `None` (ayrıca `GAMEKIT_LOG` sembolünü kaldırın)

---

## Adım 3 — GAMEKIT_LOG sembolünü etkinleştir

`GKLog.Log`, `[Conditional("GAMEKIT_LOG")]` ile işaretlenmiştir. Sembol mevcut değilse derleme zamanında **hiçbir işlem yapmaz**.

**Edit > Project Settings > Player > Scripting Define Symbols** içine ekleyin:

```
GAMEKIT_LOG
```

Release build'leri için tüm loglama yükünü ortadan kaldırmak üzere bu sembolü kaldırın.

---

## Adım 4 — Kodunuza log çağrıları ekle

```csharp
using VGameKit.Runtime.Log;

// Ayrıntılı dahili iz — release'den çıkarılır:
GKLog.Log(LogState.Development, "Alt sistem başlatılıyor...");

// Kullanıcıya yönelik oyun olayı — yeşil gösterilir:
GKLog.Log(LogState.Game, $"Oyuncu puanı: {score}");

// Process flow izi — sarı gösterilir:
GKLog.Log(LogState.ProcessFlow, "LoadLevelFlow: AsyncExecute başladı");

// Kritik olmayan uyarı — sarı gösterilir:
GKLog.Log(LogState.Warning, "Config değeri eksik, varsayılan kullanılıyor.");

// Çalışma zamanı hatası — kırmızı gösterilir:
GKLog.Log(LogState.Error, "Kayıt dosyası yüklenemedi.");
```

`GKLog.Log`, yazdırmadan önce `_logState.HasFlag(logState)` kontrolü yapar; bu nedenle devre dışı bırakılmış bir bayrak geçirmek ucuz bir boolean testidir.

---

## Adım 5 — Başlangıçta aktif log seviyesini raporla

Logger'ın hazır olduğunu doğrulamak ve aktif `LogState`'i yazdırmak için uygulama başlangıcında (örneğin `AbsAppManager.InitializeGame` içinde) `GKLog.ReportLogLevel()` çağrısı yapın:

```csharp
protected override async UniTask InitializeGame(CancellationToken token)
{
    GKLog.ReportLogLevel(); // yazdırır: [GKLog.Core] : GKLog is READY with LogState: Development
    // ...
}
```

---

## Log bayraklarını birleştirme

```csharp
// Inspector'da bayrağı toggle onay kutuları aracılığıyla seçersiniz.
// Programatik eşdeğeri:
LogState combined = LogState.Development | LogState.Game | LogState.Error;
```

`LogState` bir `[Flags]` enum'u olduğundan, `_logState.HasFlag(LogState.Game)` kontrolü, diğer bitlerin durumundan bağımsız olarak `Game` biti ayarlıyken `true` döndürür.

---

## Ayrıca Bkz.

- [Loglama referansı](../reference/api/logging.md)
- [Konfigürasyon referansı](../reference/api/configuration.md)
