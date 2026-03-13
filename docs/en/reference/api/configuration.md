# GKConfig — Configuration Reference

**Namespace:** `VGameKit.Runtime.Config`
**Assembly:** `VGameKit.Runtime`
**File:** `Assets/VGameKit/Runtime/Config/GKConfig.cs`

---

## Overview

`GKConfig` is a singleton `ScriptableObject` that stores global configuration for the VGameKit runtime. Its primary role is exposing the `LogState` flag to `GKLog`, controlling which log levels are active at runtime.

At startup, Unity loads the asset automatically via `PlayerSettings.PreloadedAssets` (Editor + builds) or falls back to `Resources.Load<GKConfig>("GKConfig")`. Only one instance is used; duplicate loads generate a `LogWarning`.

---

## Class Declaration

```csharp
public class GKConfig : ScriptableObject
```

---

## Properties

### `Instance`

```csharp
public static GKConfig Instance { get; private set; }
```

Singleton accessor. Resolution order:
1. If `_instance` is already set, return it.
2. **Editor only:** search `PlayerSettings.GetPreloadedAssets()` for the first `GKConfig` entry.
3. **Fallback (all platforms):** `Resources.Load<GKConfig>("GKConfig")`.
4. If still not found, logs a `LogWarning` and returns `null`.

---

### `LogState`

```csharp
[field: SerializeField]
public LogState LogState { get; private set; } = LogState.None;
```

Controls which `GKLog` levels are active. Set this in the Inspector on the `GKConfig` asset. The `[field: SerializeField]` attribute serializes the auto-property backing field directly.

---

## Editor Methods

### `CreateSettingsAsset` *(Editor only)*

```csharp
[MenuItem("Assets/Create/VGameKit/Config/GKConfig")]
public static void CreateSettingsAsset()
```

Accessible from the Unity menu at **Assets > Create > VGameKit > Config > GKConfig**.

Actions performed:
1. Creates `Assets/Resources/GKConfig.asset` (also creates `Assets/Resources/` if it does not exist).
2. Removes any existing `GKConfig` entry from `PlayerSettings.PreloadedAssets`.
3. Adds the new asset to `PreloadedAssets` so it loads before the first scene.
4. Saves and refreshes the `AssetDatabase`, then pings the new asset in the Project window.

---

### `LoadInstanceFromPreloadAssetsOnLoad` *(Editor only)*

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void LoadInstanceFromPreloadAssetsOnLoad()
```

Called automatically by Unity before the first scene loads (Editor Play Mode only). Forces `Instance` resolution and warns if the asset is missing.

---

## Lifecycle

### `OnEnable`

```csharp
private void OnEnable()
```

Called by Unity when the `ScriptableObject` asset is loaded. Sets `_instance = this` if no instance exists yet; warns if a second instance is loaded.

---

## Setup

1. Create the asset: **Assets > Create > VGameKit > Config > GKConfig**
2. In the Inspector, configure `LogState` (e.g., `Development | Game | Error`).
3. The asset is automatically added to **Project Settings > Player > Preloaded Assets** by the creation menu item.

> **Warning:** If the asset is not in `Resources/` and not in `PreloadedAssets`, `GKLog` will have no config and default to `LogState.None` (all logging suppressed).

---

## Usage

```csharp
// Read the current log state
var state = GKConfig.Instance.LogState;

// Check if a specific level is active
bool isDevelopmentEnabled = GKConfig.Instance.LogState.HasFlag(LogState.Development);
```

> `LogState` has a private setter — do not attempt to assign it at runtime. Configure it in the Inspector or by editing the asset.

---

## See Also

- `GKLog` — consumes `GKConfig.Instance.LogState`
- `LogState` — the flags enum stored here
