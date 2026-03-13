# GKLog — Logging Reference

**Namespace:** `VGameKit.Runtime.Log`
**Assembly:** `VGameKit.Runtime`
**File:** `Assets/VGameKit/Runtime/Log/GKLog.cs`

---

## Overview

`GKLog` is a static logging utility that wraps `UnityEngine.Debug.Log` with flag-based level filtering and color-coded Unity Console output. The entire `Log` method is stripped from release builds via the `[Conditional("GAMEKIT_LOG")]` attribute — no log calls appear in builds unless the `GAMEKIT_LOG` scripting define symbol is set.

Log level filtering is driven by `GKConfig.Instance.LogState`, a `[Flags]` enum that allows multiple levels to be active simultaneously.

---

## LogState Enum

**File:** `Assets/VGameKit/Runtime/Log/LogState.cs`
**Attribute:** `[Flags]` — values can be combined with `|`

| Value | Bit | Color | Purpose |
|---|---|---|---|
| `None` | `0` | — | Logging disabled |
| `Core` | `1 << 0` | default | Framework internals (scope ready, logger init) |
| `Development` | `1 << 1` | default | Verbose debug output during development |
| `Info` | `1 << 2` | default | General informational messages |
| `Analytics` | `1 << 3` | magenta | Analytics events |
| `IAP` | `1 << 4` | magenta | In-App Purchase events |
| `Ads` | `1 << 5` | magenta | Ad SDK events |
| `Warning` | `1 << 6` | yellow | Non-fatal warnings |
| `Game` | `1 << 7` | green | User-facing game events |
| `Pause` | `1 << 8` | blue | Pause/resume transitions |
| `ProcessFlow` | `1 << 9` | yellow | Process flow execution trace |
| `Error` | `1 << 10` | red | Runtime errors |
| `Fatal` | `1 << 11` | red | Unrecoverable failures |
| `Booster` | `1 << 12` | magenta | Booster/powerup events |
| `Timer` | `1 << 13` | magenta | Timer events |

---

## Static Class: `GKLog`

### Static Constructor

```csharp
static GKLog()
```

Reads `GKConfig.Instance.LogState` and stores it in `_logState`. Called once on first use of the class.

---

### Methods

#### `Log`

```csharp
[Conditional("GAMEKIT_LOG")]
public static void Log(LogState logState, object message)
```

Writes `message` to the Unity Console if:
1. The `GAMEKIT_LOG` scripting define is set (compile-time gate).
2. The active `_logState` has the requested `logState` flag set (`HasFlag` check).

The format is `[GKLog.<Level>] : <message>`. Color wrapping is applied based on the level group (see table above).

| Parameter | Type | Description |
|---|---|---|
| `logState` | `LogState` | The level/category for this message |
| `message` | `object` | Message body; `ToString()` is called implicitly |

> **Note:** Because the method is `[Conditional("GAMEKIT_LOG")]`, the compiler removes all call sites when `GAMEKIT_LOG` is not defined. The `message` expression is not evaluated — no allocations occur in production builds.

---

#### `ReportLogLevel`

```csharp
public static void ReportLogLevel()
```

Logs the current `LogState` at `LogState.Core`. Called automatically by `AbsMainLifetimeScope.Configure` when `GAMEKIT_LOG` is defined. Useful to confirm the logger is active and which levels are enabled.

---

## Enabling Logging

Add `GAMEKIT_LOG` to **Project Settings > Player > Scripting Define Symbols** for the target platform. Without this symbol the `Log` method body is completely removed from all builds.

To control which levels appear, set `LogState` on the `GKConfig` ScriptableObject asset (create via **Assets > Create > VGameKit > Config > GKConfig**).

### Example: enable all levels

```csharp
// In GKConfig asset inspector, set LogState to all flags:
// Core | Development | Info | Warning | Game | Error | Fatal | ...
```

### Typical usage

```csharp
using VGameKit.Runtime.Log;

// Verbose debug — stripped in release
GKLog.Log(LogState.Development, $"Pool size after spawn: {_pool.Count}");

// User-visible game event
GKLog.Log(LogState.Game, "Level complete");

// Async flow trace
GKLog.Log(LogState.ProcessFlow, "DemoFlow: start");
await DoWorkAsync(token);
GKLog.Log(LogState.ProcessFlow, "DemoFlow: end");

// Runtime error
GKLog.Log(LogState.Error, $"Failed to load config: {ex.Message}");
```

---

## Output Format

```
[GKLog.Development] : Pool size after spawn: 4
<color=green>[GKLog.Game] : Level complete</color>
<color=yellow>[GKLog.ProcessFlow] : DemoFlow: start</color>
<color=red>[GKLog.Error] : Failed to load config: File not found</color>
```

---

## See Also

- `GKConfig` — stores the active `LogState`
- `LogState` — flags enum controlling which levels are active
- `AbsMainLifetimeScope` — calls `GKLog.ReportLogLevel()` during DI container build
