# JSonKit Reference

**Namespace:** `VGameKit.IO.Runtime`
**Assembly:** `VGameKit.IO.Runtime`
**File:** `Assets/VGameKit.IO/Runtime/JSonKit.cs`

---

## Overview

`JSonKit` is a static utility class that wraps `Newtonsoft.Json` (`JsonConvert`) for file-based JSON persistence. It provides a consistent API for saving and loading typed data across all Unity platforms, with special handling for Android's `StreamingAssets` paths via `UnityWebRequest` and UniTask.

---

## Class Declaration

```csharp
public static class JSonKit
```

---

## Methods

### `Save<T>`

```csharp
public static void Save<T>(string filePath, T value, Formatting formatting = Formatting.None)
```

Serializes `value` to JSON and writes it to `filePath`.

- Creates the directory if it does not exist.
- Encoding: UTF-8 without BOM (`new UTF8Encoding(false)`).
- Uses `Newtonsoft.Json.JsonConvert.SerializeObject`.
- `formatting`: `Formatting.None` (compact, default) or `Formatting.Indented` (human-readable).

| Parameter | Type | Description |
|---|---|---|
| `filePath` | `string` | Absolute or `Application.persistentDataPath`-relative path |
| `value` | `T` | Object to serialize |
| `formatting` | `Formatting` | JSON output formatting |

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

Reads the file at `filePath` and deserializes it to `T`.

- Returns `default(T)` if the file content is an empty string.
- Uses `Newtonsoft.Json.JsonConvert.DeserializeObject<T>`.

| Parameter | Type | Description |
|---|---|---|
| `filePath` | `string` | Path to the JSON file |
| `token` | `CancellationToken` | Cancellation token |

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

Platform-aware async file reader:

| Platform | Implementation |
|---|---|
| `UNITY_ANDROID` | `UnityWebRequest` + `await` for URI paths; `File.ReadAllText` for local paths |
| Other | `File.ReadAllText(filePath)` |

| Parameter | Type | Description |
|---|---|---|
| `filePath` | `string` | Path or URI to read from |
| `token` | `CancellationToken` | Cancellation token |

---

## Usage Pattern

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

## See Also

- `GKConfig` — ScriptableObject config (not file-based)
- Newtonsoft.Json — `JsonConvert`, `Formatting`
