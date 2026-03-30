# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VGameKit is a modular Unity game development framework (Unity 6000.3.7f1) built on three pillars:
- **VContainer** (v1.17.0) — Dependency injection and object lifetime management
- **UniTask** (v2.5.10) — Async/await for Unity (never use `System.Threading.Tasks.Task`)
- **MessagePipe** (v1.8.1) — In-process pub/sub messaging

The codebase is organized into four assembly modules: `VGameKit.Runtime` (core), `VGameKit.IO.Runtime` (JSON), `VGameKit.GA.Runtime` (GameAnalytics), and `VGameKit.GoogleAds.Runtime` (ads).

## Commands

```bash
# Build (all warnings treated as errors)
dotnet build VGameKit.sln -c Debug

# Lint check (no fixes)
dotnet format VGameKit.sln --verify-no-changes

# Lint with fixes
dotnet format VGameKit.sln

# Wiki validation
python3 scripts/publish_wiki.py --dry-run  # Expected: 63 pages, 0 errors

# MCP server: rebuild search index (run after docs/en/ changes)
cd mcp-server && uv run python -m vgamekit_mcp.build_index
```

**Tests:** The test framework (`com.unity.test-framework` v1.6.0) is installed but no test assemblies exist yet. Smoke test: open `Assets/Demo/Scenes/SampleScene.unity`, enter Play Mode, verify GKLog output in Console.

**Unity batch-mode tests (when assemblies exist):**
```bash
/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "$PWD" \
  -runTests -testPlatform editmode \
  -testFilter MyTestClass.My_method_description \
  -logFile Logs/Editor-single.log
```

## Code Style

**Braces:** Allman style (opening brace on its own line).

**Import order:** `System.*` → `Cysharp.*` → `MessagePipe` → `VContainer` → `UnityEngine` → `VGameKit`

**Naming:**
- Private/protected fields: `_camelCase`
- Types, enums, interfaces, methods, properties: `PascalCase`
- Interfaces prefixed with `I`

**XML docs:** Required `<summary>` on all public/protected members; `<typeparam>` on public generics.

**Async:** Use `UniTask`/`UniTaskVoid` exclusively. Always pass `CancellationToken` through chains. Use `.AttachExternalCancellation(token).SuppressCancellationThrow()` for cancellation handling. Never call `.GetAwaiter().GetResult()`.

**Logging:** `GKLog.Log(LogState.<level>, message)` — decorated with `[Conditional("GAMEKIT_LOG")]` so calls are stripped at compile time unless `GAMEKIT_LOG` is defined (set for Standalone; must be added manually for Android/iOS).

## Architecture

### Dependency Injection (VContainer)

**`AbsMainLifetimeScope`** — the single root DI scope for the app. It calls `RegisterMessagePipe()` internally — **never call it again** in sub-scopes. Scene/subsystem scopes extend `AbsBaseLifetimeScope`.

Critical registration rules:
- Always add `AsImplementedInterfaces()` so VContainer calls `IInitializable.Initialize()` and `IDisposable.Dispose()` automatically
- Add `.AsSelf()` when the concrete type is also injected directly
- For MonoBehaviours, always use `builder.RegisterComponent(_myMB)` (never `RegisterComponentInHierarchy`)
- Process flows require `ProcessFlowProvider` registered first, then `builder.RegisterProcessFlow<TArgs, TFlow>(Lifetime.Singleton)`

### Service Base Classes

**`SubscribableConcrete`** — base for plain-C# services using MessagePipe. Override `Init()` for initialization logic and `Subscriptions()` to register subscriptions (both called by `Initialize()` — never call `Initialize()` directly). Subscriptions are added via `.AddTo(_bagBuilder)`.

**`SubscribableMonoBehaviour`** — MonoBehaviour version; VContainer injects via `[Inject] void Construct(...)`.

### Application Lifecycle

**`AbsAppManager`** seals `StartAsync()`. Only override `InitializeGame(CancellationToken)` and `OnAppReady(AppReadyEvent)`. It publishes `AppReadyEvent` on completion; other systems subscribe to this to know when to activate.

### Process Flows

Sequential async task chains. Implement by extending `BaseProcessFlow<TArgs>` and overriding `AsyncExecute(CancellationToken)`.

```csharp
// Caller owns the token — never use flow.cancellationToken
[Inject] private readonly Func<MyFlowArgs, MyFlow> _myFlowFactory;
var flow = _myFlowFactory(new MyFlowArgs(data));
flow.OnComplete(f => { /* handle result */ });
flow.Execute(_cts.Token);
```

### UI Menu System

`BaseMenuManager<TMenuName>` is generic on an enum type. Open menus by publishing events, never by calling `OpenMenu()` directly. `MenuMode.Single` auto-closes other menus.

```csharp
// Correct: publish to open
_menuPublisher.Publish(new OpenMainMenuEvent());

// Never: do not call directly from external code
menuManager.OpenMenu(...);
```

`BaseCloseOthersMenuEvent<T>` — constructor body must stay **empty**; define `KeepMenuNames` as a property in the subclass.

### Spawner / Object Pooling

`BaseSpawnPool<TModel, TItem>` — generic pool where `TModel : ISpawnItemModel` (class, not struct) and `TItem : MonoBehaviour, ISpawnItem<TModel>`. On teardown, call `Dispose()` (which calls `RemoveAllObjects()`); never call `HideAllObjects()` in teardown.

### Compilation Symbols

| Symbol | Purpose |
|---|---|
| `GAMEKIT_LOG` | Gates all `GKLog` calls; set for Standalone, manual for mobile |
| `GA_ENABLED` | Auto-defined by `versionDefines` when GameAnalytics SDK present |
| `VGameKIT_GA` | Auto-defined by `versionDefines` in VGameKit.Runtime.asmdef |
| `GOOGLEADS_TESTDEVICE` | Must be added manually per-project for ad testing |

## Known Bugs

## Documentation

- `AGENTS.md` — authoritative developer/agent handbook (DI examples, code style, pre-PR checklist)
- `docs/en/` and `docs/tr/` — bilingual documentation (Diátaxis structure). Always update both together for any public API changes.
- Run `python3 scripts/publish_wiki.py --dry-run` before PRs — must report 63 pages, 0 errors.

## Git

`Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln` are all git-ignored (regenerated by Unity). Never commit them. Use `git add -p` for Unity serialized-file changes.

## Pre-PR Checklist

1. `dotnet build VGameKit.sln -c Debug` → zero warnings
2. `dotnet format VGameKit.sln --verify-no-changes` → no drift
3. Play Mode smoke test in `SampleScene.unity` → GKLog output visible
4. Docs updated in both `docs/en/` and `docs/tr/` for any public API changes
5. `python3 scripts/publish_wiki.py --dry-run` → 63 pages, 0 errors
6. If `docs/en/` changed: `cd mcp-server && uv run python -m vgamekit_mcp.build_index`, then `git add src/vgamekit_mcp/data/`
