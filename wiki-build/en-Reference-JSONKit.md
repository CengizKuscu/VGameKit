# JSonKit Reference

**Namespace:** `VGameKit.IO.Runtime`
**Assembly:** `VGameKit.IO.Runtime`
**File:** `Assets/VGameKit.IO/Runtime/JSonKit.cs`

---

## Overview

`JSonKit` is a static utility class that wraps `Newtonsoft.Json` (`JsonConvert`) for file-based JSON persistence. It provides a consistent API for saving and loading typed data across all Unity platforms, with special handling for Android's `StreamingAssets` paths.

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
// Save player data
JSonKit.Save(
    Path.Combine(Application.persistentDataPath, "save.json"),
    new PlayerData { Score = 1500, Level = 3 });
```

---

### `Load<T>`

```csharp
public static T Load<T>(string filePath)
```

Reads the file at `filePath` and deserializes it to `T`.

- Returns `default(T)` if the file content is an empty string.
- Uses `Newtonsoft.Json.JsonConvert.DeserializeObject<T>`.

| Parameter | Type | Description |
|---|---|---|
| `filePath` | `string` | Path to the JSON file |

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

Platform-aware file reader:

| Platform | Implementation |
|---|---|
| `UNITY_EDITOR` | `File.ReadAllText(filePath)` |
| `UNITY_IOS` | `File.ReadAllText(filePath)` |
| `UNITY_ANDROID` | `ReadAllTextOnAndroid(filePath)` — see note below |
| Other | `File.ReadAllText(filePath)` |

---

### `ReadAllTextOnAndroid` *(private)*

```csharp
private static string ReadAllTextOnAndroid(string filePath)
```

- If `filePath` contains `"://"` (i.e. a streaming assets URI): uses the deprecated `WWW` class with a **busy-wait loop** (`while (!www.isDone) {}`) — this blocks the main thread.
- Otherwise: `File.ReadAllText(filePath)`.

> **Known issue:** The `WWW`-based path blocks the Unity main thread. For streaming assets on Android, use `UnityWebRequest` with `async/await` or coroutines instead. This issue exists in the current codebase and is a candidate for replacement.

---

## Usage Pattern

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

## See Also

- `GKConfig` — ScriptableObject config (not file-based)
- Newtonsoft.Json — `JsonConvert`, `Formatting`
