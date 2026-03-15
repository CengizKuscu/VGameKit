# JSonKit Referansı

**Ad Alanı:** `VGameKit.IO.Runtime`
**Derleme:** `VGameKit.IO.Runtime`
**Dosya:** `Assets/VGameKit.IO/Runtime/JSonKit.cs`

---

## Genel Bakış

`JSonKit`, dosya tabanlı JSON kalıcılığı için `Newtonsoft.Json` (`JsonConvert`) sarmalayan statik bir yardımcı sınıftır. Tüm Unity platformlarında tiplendirilmiş veri kaydetme ve yükleme için tutarlı bir API sunar; Android'deki `StreamingAssets` yolları için özel işlem sağlar.

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
// Oyuncu verisini kaydet
JSonKit.Save(
    Path.Combine(Application.persistentDataPath, "save.json"),
    new PlayerData { Score = 1500, Level = 3 });
```

---

### `Load<T>`

```csharp
public static T Load<T>(string filePath)
```

`filePath`'deki dosyayı okur ve `T`'ye seri halden çıkarır.

- Dosya içeriği boş dizeyse `default(T)` döndürür.
- `Newtonsoft.Json.JsonConvert.DeserializeObject<T>` kullanır.

| Parametre | Tip | Açıklama |
|---|---|---|
| `filePath` | `string` | JSON dosyasına giden yol |

```csharp
var data = JSonKit.Load<PlayerData>(
    Path.Combine(Application.persistentDataPath, "save.json"));

if (data != null)
{
    ApplyPlayerData(data);
}
```

---

### `ReadAllText`

```csharp
public static string ReadAllText(string filePath)
```

Platform duyarlı dosya okuyucu:

| Platform | Uygulama |
|---|---|
| `UNITY_EDITOR` | `File.ReadAllText(filePath)` |
| `UNITY_IOS` | `File.ReadAllText(filePath)` |
| `UNITY_ANDROID` | `ReadAllTextOnAndroid(filePath)` — aşağıdaki nota bakın |
| Diğer | `File.ReadAllText(filePath)` |

---

### `ReadAllTextOnAndroid` *(özel)*

```csharp
private static string ReadAllTextOnAndroid(string filePath)
```

- `filePath` `"://"` içeriyorsa (yani bir streaming assets URI ise): Ana iş parçacığını **bloke eden meşgul bekleme döngüsü** (`while (!www.isDone) {}`) ile kullanım dışı `WWW` sınıfını kullanır.
- Aksi hâlde: `File.ReadAllText(filePath)`.

> **Bilinen sorun:** `WWW` tabanlı yol Unity ana iş parçacığını bloke eder. Android'de streaming assets için bunun yerine `async/await` veya coroutine ile `UnityWebRequest` kullanın. Bu sorun mevcut kod tabanında mevcuttur ve değiştirilmesi adaydır.

---

## Kullanım Kalıbı

```csharp
using System.IO;
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

    public static GameSettings Load()
    {
        return JSonKit.Load<GameSettings>(FilePath) ?? new GameSettings();
    }
}
```

---

## Ayrıca Bakınız

- `GKConfig` — ScriptableObject yapılandırması (dosya tabanlı değil)
- Newtonsoft.Json — `JsonConvert`, `Formatting`
