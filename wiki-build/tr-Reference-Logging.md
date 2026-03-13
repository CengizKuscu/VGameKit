# GKLog — Loglama Referansı

**Namespace:** `VGameKit.Runtime.Log`
**Assembly:** `VGameKit.Runtime`
**Dosya:** `Assets/VGameKit/Runtime/Log/GKLog.cs`

---

## Genel Bakış

`GKLog`, `UnityEngine.Debug.Log` üzerinde flag tabanlı seviye filtrelemesi ve renk kodlu Unity Console çıktısı sağlayan statik bir loglama yardımcısıdır. `Log` metodunun tamamı `[Conditional("GAMEKIT_LOG")]` attribute'u sayesinde release build'lerden kaldırılır — `GAMEKIT_LOG` scripting define sembolü tanımlı olmadıkça build'larda hiçbir log çağrısı yer almaz.

Log seviyesi filtrelemesi `GKConfig.Instance.LogState` tarafından yönetilir; bu `[Flags]` enum birden fazla seviyenin aynı anda etkin olmasına izin verir.

---

## LogState Enum

**Dosya:** `Assets/VGameKit/Runtime/Log/LogState.cs`
**Attribute:** `[Flags]` — değerler `|` ile birleştirilebilir

| Değer | Bit | Renk | Amaç |
|---|---|---|---|
| `None` | `0` | — | Loglama devre dışı |
| `Core` | `1 << 0` | varsayılan | Framework dahilileri (scope hazır, logger init) |
| `Development` | `1 << 1` | varsayılan | Geliştirme sırasında ayrıntılı debug çıktısı |
| `Info` | `1 << 2` | varsayılan | Genel bilgi mesajları |
| `Analytics` | `1 << 3` | magenta | Analitik olayları |
| `IAP` | `1 << 4` | magenta | Uygulama içi satın alma olayları |
| `Ads` | `1 << 5` | magenta | Reklam SDK olayları |
| `Warning` | `1 << 6` | sarı | Ölümcül olmayan uyarılar |
| `Game` | `1 << 7` | yeşil | Kullanıcıya yönelik oyun olayları |
| `Pause` | `1 << 8` | mavi | Duraklatma/devam etme geçişleri |
| `ProcessFlow` | `1 << 9` | sarı | Process flow yürütme izleme |
| `Error` | `1 << 10` | kırmızı | Çalışma zamanı hataları |
| `Fatal` | `1 << 11` | kırmızı | Kurtarılamaz hatalar |
| `Booster` | `1 << 12` | magenta | Güçlendirici/powerup olayları |
| `Timer` | `1 << 13` | magenta | Zamanlayıcı olayları |

---

## Statik Sınıf: `GKLog`

### Statik Kurucu

```csharp
static GKLog()
```

`GKConfig.Instance.LogState`'i okur ve `_logState`'e depolar. Sınıfa ilk erişimde bir kez çağrılır.

---

### Metodlar

#### `Log`

```csharp
[Conditional("GAMEKIT_LOG")]
public static void Log(LogState logState, object message)
```

Şu koşullar sağlandığında `message`'ı Unity Console'a yazar:
1. `GAMEKIT_LOG` scripting define tanımlıdır (derleme zamanı kapısı).
2. Aktif `_logState`, istenen `logState` flag'ini içermektedir (`HasFlag` kontrolü).

Format: `[GKLog.<Seviye>] : <mesaj>`. Renk sarması seviye grubuna göre uygulanır (yukarıdaki tabloya bakın).

| Parametre | Tür | Açıklama |
|---|---|---|
| `logState` | `LogState` | Bu mesajın seviyesi/kategorisi |
| `message` | `object` | Mesaj içeriği; `ToString()` örtük olarak çağrılır |

> **Not:** Metot `[Conditional("GAMEKIT_LOG")]` ile işaretlendiğinden, `GAMEKIT_LOG` tanımlı değilse derleyici tüm çağrı noktalarını kaldırır. `message` ifadesi değerlendirilmez — production build'larda hiç bellek tahsisi oluşmaz.

---

#### `ReportLogLevel`

```csharp
public static void ReportLogLevel()
```

Mevcut `LogState`'i `LogState.Core` seviyesinde loglar. `GAMEKIT_LOG` tanımlı olduğunda `AbsMainLifetimeScope.Configure` tarafından otomatik olarak çağrılır. Logger'ın etkin olduğunu ve hangi seviyelerin aktif olduğunu doğrulamak için kullanışlıdır.

---

## Loglama Etkinleştirme

Hedef platform için **Project Settings > Player > Scripting Define Symbols** kısmına `GAMEKIT_LOG` ekleyin. Bu sembol olmadan `Log` metot gövdesi tüm build'lardan tamamen kaldırılır.

Hangi seviyelerin görüneceğini kontrol etmek için `GKConfig` ScriptableObject asset'indeki (oluşturmak için **Assets > Create > VGameKit > Config > GKConfig**) `LogState` alanını ayarlayın.

### Örnek: tüm seviyeleri etkinleştirme

```csharp
// GKConfig asset inspector'ında LogState'i tüm flag'lere ayarlayın:
// Core | Development | Info | Warning | Game | Error | Fatal | ...
```

### Tipik kullanım

```csharp
using VGameKit.Runtime.Log;

// Ayrıntılı debug — release'de kaldırılır
GKLog.Log(LogState.Development, $"Spawn sonrası pool boyutu: {_pool.Count}");

// Kullanıcıya yönelik oyun olayı
GKLog.Log(LogState.Game, "Bölüm tamamlandı");

// Async akış izleme
GKLog.Log(LogState.ProcessFlow, "DemoFlow: başlıyor");
await DoWorkAsync(token);
GKLog.Log(LogState.ProcessFlow, "DemoFlow: bitti");

// Çalışma zamanı hatası
GKLog.Log(LogState.Error, $"Config yüklenemedi: {ex.Message}");
```

---

## Çıktı Formatı

```
[GKLog.Development] : Spawn sonrası pool boyutu: 4
<color=green>[GKLog.Game] : Bölüm tamamlandı</color>
<color=yellow>[GKLog.ProcessFlow] : DemoFlow: başlıyor</color>
<color=red>[GKLog.Error] : Config yüklenemedi: Dosya bulunamadı</color>
```

---

## Ayrıca Bkz.

- `GKConfig` — aktif `LogState`'i depolar
- `LogState` — hangi seviyelerin etkin olduğunu kontrol eden flags enum
- `AbsMainLifetimeScope` — DI container oluşturma sırasında `GKLog.ReportLogLevel()` çağırır
