# How to Configure Logging

## Overview

VGameKit's logging system is controlled by `GKConfig` (a `ScriptableObject`) and `GKLog` (a static utility). Log output is compiled away entirely unless the `GAMEKIT_LOG` scripting define symbol is present. This guide covers creating the config asset, choosing log levels, and adding log calls in code.

**Prerequisites:** `VGameKit.Runtime` module installed.

---

## Step 1 — Create the GKConfig asset

In the Unity Editor, go to:

**Assets > Create > VGameKit > Config > GKConfig**

This creates `Assets/Resources/GKConfig.asset` and automatically adds it to **Player Settings > Preloaded Assets**. Only one `GKConfig` asset is ever active at runtime.

---

## Step 2 — Set the log state

Select the `GKConfig` asset in the Project window and set the **Log State** field in the Inspector.

`LogState` is a `[Flags]` enum — combine values with the bitwise OR operator to enable multiple categories simultaneously.

| Flag | Category | Unity console colour |
|------|----------|---------------------|
| `None` | Logging disabled | — |
| `Core` | Framework internals | default |
| `Development` | Verbose debug output | default |
| `Info` | General informational messages | default |
| `Analytics` | Analytics events | magenta |
| `IAP` | In-app purchase events | magenta |
| `Ads` | Ad lifecycle events | magenta |
| `Warning` | Non-fatal warnings | yellow |
| `Game` | User-facing game events | green |
| `Pause` | Pause/resume transitions | blue |
| `ProcessFlow` | Process flow execution trace | yellow |
| `Error` | Runtime errors | red |
| `Fatal` | Unrecoverable errors | red |
| `Booster` | Booster/power-up events | magenta |
| `Timer` | Timer events | magenta |

**Recommended development configuration:** `Development | Game | ProcessFlow | Error | Warning`

**Recommended release configuration:** `None` (plus removing the `GAMEKIT_LOG` symbol)

---

## Step 3 — Enable the GAMEKIT_LOG symbol

`GKLog.Log` is decorated with `[Conditional("GAMEKIT_LOG")]`. It is a **no-op** at compile time unless the symbol exists.

Add it in **Edit > Project Settings > Player > Scripting Define Symbols**:

```
GAMEKIT_LOG
```

Remove it for release builds to eliminate all logging overhead.

---

## Step 4 — Add log calls in your code

```csharp
using VGameKit.Runtime.Log;

// Verbose internal trace — stripped from release:
GKLog.Log(LogState.Development, "Initialising subsystem...");

// User-impacting game event — shown in green:
GKLog.Log(LogState.Game, $"Player scored: {score}");

// Process flow trace — shown in yellow:
GKLog.Log(LogState.ProcessFlow, "LoadLevelFlow: AsyncExecute start");

// Non-fatal warning — shown in yellow:
GKLog.Log(LogState.Warning, "Config value missing, using default.");

// Runtime error — shown in red:
GKLog.Log(LogState.Error, "Failed to load save file.");
```

`GKLog.Log` checks `_logState.HasFlag(logState)` before printing, so passing a disabled flag is a cheap boolean test.

---

## Step 5 — Report the active log level at startup

Call `GKLog.ReportLogLevel()` early in your app startup (e.g., inside `AbsAppManager.InitializeGame`) to confirm the logger is ready and print the active `LogState`:

```csharp
protected override async UniTask InitializeGame(CancellationToken token)
{
    GKLog.ReportLogLevel(); // prints: [GKLog.Core] : GKLog is READY with LogState: Development
    // ...
}
```

---

## Combining log flags

```csharp
// In the Inspector you select flags via toggle checkboxes.
// Programmatically, the equivalent is:
LogState combined = LogState.Development | LogState.Game | LogState.Error;
```

Because `LogState` is a `[Flags]` enum, checking `_logState.HasFlag(LogState.Game)` returns `true` whenever the `Game` bit is set, regardless of what other bits are set.

---

## See also

- [Logging reference](../reference/api/logging.md)
- [Configuration reference](../reference/api/configuration.md)
