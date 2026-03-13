# GKConfig — Konfigürasyon Referansı

**Namespace:** `VGameKit.Runtime.Config`
**Assembly:** `VGameKit.Runtime`
**Dosya:** `Assets/VGameKit/Runtime/Config/GKConfig.cs`

---

## Genel Bakış

`GKConfig`, VGameKit runtime için global konfigürasyonu depolayan bir singleton `ScriptableObject`'tir. Temel görevi, hangi log seviyelerinin çalışma zamanında etkin olduğunu kontrol eden `LogState` flag'ini `GKLog`'a sunmaktır.

Başlangıçta Unity, asset'i `PlayerSettings.PreloadedAssets` üzerinden otomatik olarak yükler (Editor + build'lar) ya da `Resources.Load<GKConfig>("GKConfig")` yedek yolunu kullanır. Yalnızca bir instance kullanılır; çift yükleme durumunda `LogWarning` üretilir.

---

## Sınıf Tanımı

```csharp
public class GKConfig : ScriptableObject
```

---

## Özellikler

### `Instance`

```csharp
public static GKConfig Instance { get; private set; }
```

Singleton erişimcisi. Çözümleme sırası:
1. `_instance` zaten ayarlanmışsa onu döndür.
2. **Yalnızca Editor:** `PlayerSettings.GetPreloadedAssets()` içinde ilk `GKConfig` girişini ara.
3. **Yedek (tüm platformlar):** `Resources.Load<GKConfig>("GKConfig")`.
4. Hâlâ bulunamazsa `LogWarning` loglar ve `null` döndürür.

---

### `LogState`

```csharp
[field: SerializeField]
public LogState LogState { get; private set; } = LogState.None;
```

Hangi `GKLog` seviyelerinin etkin olduğunu kontrol eder. `GKConfig` asset'i üzerindeki Inspector'da ayarlanır. `[field: SerializeField]` attribute'u, auto-property backing field'ı doğrudan serialize eder.

---

## Editor Metodları

### `CreateSettingsAsset` *(Yalnızca Editor)*

```csharp
[MenuItem("Assets/Create/VGameKit/Config/GKConfig")]
public static void CreateSettingsAsset()
```

Unity menüsünden **Assets > Create > VGameKit > Config > GKConfig** yoluyla erişilir.

Gerçekleştirilen işlemler:
1. `Assets/Resources/GKConfig.asset` oluşturur (`Assets/Resources/` yoksa oluşturur).
2. `PlayerSettings.PreloadedAssets`'ten mevcut `GKConfig` girişlerini kaldırır.
3. Yeni asset'i `PreloadedAssets`'e ekler; böylece ilk scene yüklenmeden önce yüklenir.
4. `AssetDatabase`'i kaydeder ve yeniler, ardından Project penceresinde yeni asset'i ping'ler.

---

### `LoadInstanceFromPreloadAssetsOnLoad` *(Yalnızca Editor)*

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void LoadInstanceFromPreloadAssetsOnLoad()
```

Unity tarafından ilk scene yüklenmeden önce otomatik çağrılır (yalnızca Editor Play Mode). `Instance` çözümlemesini zorlar ve asset eksikse uyarır.

---

## Yaşam Döngüsü

### `OnEnable`

```csharp
private void OnEnable()
```

`ScriptableObject` asset'i yüklendiğinde Unity tarafından çağrılır. Henüz instance yoksa `_instance = this` atar; ikinci bir instance yüklenirse uyarı verir.

---

## Kurulum

1. Asset oluşturun: **Assets > Create > VGameKit > Config > GKConfig**
2. Inspector'da `LogState` değerini yapılandırın (örn. `Development | Game | Error`).
3. Asset, oluşturma menü öğesi tarafından otomatik olarak **Project Settings > Player > Preloaded Assets**'e eklenir.

> **Uyarı:** Asset `Resources/` içinde değilse ve `PreloadedAssets`'e eklenmemişse `GKLog`'un konfigürasyonu olmaz ve varsayılan olarak `LogState.None` kullanılır (tüm loglama bastırılır).

---

## Kullanım

```csharp
// Mevcut log state'ini oku
var state = GKConfig.Instance.LogState;

// Belirli bir seviyenin etkin olup olmadığını kontrol et
bool isDevelopmentEnabled = GKConfig.Instance.LogState.HasFlag(LogState.Development);
```

> `LogState`'in private setter'ı vardır — çalışma zamanında atamaya çalışmayın. Inspector'da veya asset'i düzenleyerek yapılandırın.

---

## Ayrıca Bkz.

- `GKLog` — `GKConfig.Instance.LogState`'i tüketir
- `LogState` — burada depolanan flags enum
