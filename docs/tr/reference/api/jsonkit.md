# JSonKit Referansı

**Ad Alanı:** `VGameKit.IO.Runtime`
**Derleme:** `VGameKit.IO.Runtime`
**Dosya:** `Assets/VGameKit.IO/Runtime/JSonKit.cs`

---

## Genel Bakış

`JSonKit`, dosya tabanlı JSON kalıcılığı için `Newtonsoft.Json` (`JsonConvert`) sarmalayan statik bir yardımcı sınıftır. Tüm Unity platformlarında tiplendirilmiş veri kaydetme ve yükleme için tutarlı bir API sunar; Android'deki `StreamingAssets` yolları `UnityWebRequest` ve UniTask ile non-blocking olarak işlenir.

---

## Sınıf Tanımı

```csharp
public static class JSonKit
```

---

## Metotlar

### `Save<T>`

```csharp
public static void Save<T>(string filePath, T value, Formatting formatting = Formatting.None)
```

`value`'yu JSON'a seri hale getirir ve `filePath`'e yazar.

- Dizin yoksa oluşturur.
- Kodlama: BOM olmadan UTF-8 (`new UTF8Encoding(false)`).
- `Newtonsoft.Json.JsonConvert.SerializeObject` kullanır.
- `formatting`: `Formatting.None` (sıkıştırılmış, varsayılan) veya `Formatting.Indented` (okunabilir).

| Parametre | Tip | Açıklama |
|---|---|---|
| `filePath` | `string` | Mutlak veya `Application.persistentDataPath` göreli yol |
| `value` | `T` | Seri hale getirilecek nesne |
| `formatting` | `Formatting` | JSON çıktı biçimlendirmesi |

```csharp
JSonKit.Save(
    Path.Combine(Application.persistentDataPath, "save.json"),
    new PlayerData { Score = 1500, Level = 3 });
```

---

### `LoadAsync<T>`

```csharp
public static async UniTask<T> LoadAsync<T>(string filePath, CancellationToken token)
```

`filePath`'deki dosyayı okur ve `T`'ye seri halden çıkarır.

- Dosya içeriği boş dizeyse `default(T)` döndürür.
- `Newtonsoft.Json.JsonConvert.DeserializeObject<T>` kullanır.

| Parametre | Tip | Açıklama |
|---|---|---|
| `filePath` | `string` | JSON dosyasına giden yol |
| `token` | `CancellationToken` | İptal token'ı |

```csharp
var data = await JSonKit.LoadAsync<PlayerData>(
    Path.Combine(Application.persistentDataPath, "save.json"), token);

if (data != null)
{
    ApplyPlayerData(data);
}
```

---

### `ReadAllTextAsync`

```csharp
public static async UniTask<string> ReadAllTextAsync(string filePath, CancellationToken token)
```

Platform duyarlı async dosya okuyucu:

| Platform | Uygulama |
|---|---|
| `UNITY_ANDROID` | URI yolları için `UnityWebRequest` + `await`; yerel yollar için `File.ReadAllText` |
| Diğer | `File.ReadAllText(filePath)` |

| Parametre | Tip | Açıklama |
|---|---|---|
| `filePath` | `string` | Okunacak yol veya URI |
| `token` | `CancellationToken` | İptal token'ı |

---

## Kullanım Kalıbı

```csharp
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VGameKit.IO.Runtime;
using Newtonsoft.Json;

[System.Serializable]
public class GameSettings
{
    public float MusicVolume = 1f;
    public float SfxVolume = 1f;
    public bool Vibration = true;
}

public static class SettingsManager
{
    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "settings.json");

    public static void Save(GameSettings settings)
    {
        JSonKit.Save(FilePath, settings, Formatting.Indented);
    }

    public static async UniTask<GameSettings> LoadAsync(CancellationToken token)
    {
        return await JSonKit.LoadAsync<GameSettings>(FilePath, token) ?? new GameSettings();
    }
}
```

---

## Ayrıca Bakınız

- `GKConfig` — ScriptableObject yapılandırması (dosya tabanlı değil)
- Newtonsoft.Json — `JsonConvert`, `Formatting`
