# VGameKit Agent Handbook

This playbook keeps autonomous agents aligned when working inside this repo. Unity projects punish improvisation — follow it precisely.

## Project Snapshot
- **Engine**: Unity 6000.3.7f1 (`ProjectSettings/ProjectVersion.txt`, revision `696ec25a53d1`).
- **Language**: C# 9, .NET 4.7.1 (Unity Mono), one asmdef per module under `Assets/VGameKit*`.
- **Key deps**: VContainer 1.17.0 (DI), MessagePipe 1.8.1 (pub/sub), UniTask 2.5.10 (async).
- **Compile symbols**:
  - `GAMEKIT_LOG` — gates all `GKLog` calls; set for Standalone only — add manually for Android/iOS.
  - `GOOGLEADS_TESTDEVICE` — not in ProjectSettings; add manually per-project.
  - `GA_ENABLED` — auto via `versionDefines` in `VGameKit.GoogleAds.Runtime.asmdef` when `com.gameanalytics.sdk` present.
  - `VGameKIT_GA` — auto via `versionDefines` in `VGameKit.Runtime.asmdef` when `com.gameanalytics.sdk` present.
- **Cursor/Copilot rules**: none (`.cursor/**`, `.cursorrules`, `.github/copilot-instructions.md` absent); this file is authoritative.

## Repository Layout
```
Assets/VGameKit/Runtime/      Core: App, Config, Core, Log, ProcessFlows, Spawner, UI, Utilities
Assets/VGameKit.GA/Runtime/   GameAnalytics glue
Assets/VGameKit.GoogleAds/    Ads controllers, consent, editor tooling
Assets/VGameKit.IO/Runtime/   JSonKit serialization helpers
Assets/Demo/                  Example scopes, managers, scenes (SampleScene.unity)
docs/en/ + docs/tr/           Bilingual Diátaxis docs — always edit both languages together
scripts/publish_wiki.py       Flat-file wiki builder; --dry-run must output 63 pages, 0 errors
ProjectSettings/, Packages/   Unity-managed — do not hand-edit
```

## Build & Lint
```bash
# VGameKit.sln and *.csproj are gitignored; Unity regenerates them on project open.
dotnet build VGameKit.sln -c Debug              # full solution — treat all warnings as errors
dotnet build VGameKit.Runtime.csproj -c Debug   # single assembly
dotnet format VGameKit.sln --verify-no-changes  # drop flag to apply fixes
python3 scripts/publish_wiki.py --dry-run       # wiki validation

# Unity player build (macOS)
/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "$PWD" -buildTarget StandaloneOSX \
  -customBuildName VGameKitDemo -logFile Logs/Editor-build.log
```
Never delete `Library/` in a shared worktree.

## Tests
No formal test assemblies yet (`com.unity.test-framework` 1.6.0 installed but unused). When added:
```bash
# Edit-mode — all tests
/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "$PWD" \
  -runTests -testPlatform editmode -logFile Logs/Editor-editmode.log

# Single test — use unique ClassName.MethodName
/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "$PWD" \
  -runTests -testPlatform editmode \
  -testFilter MyTestClass.My_method_description \
  -logFile Logs/Editor-single.log
```
Smoke test: open `Assets/Demo/Scenes/SampleScene.unity`, enter Play Mode, confirm `GKLog` output in Console.

## Code Style

**Braces**: Allman style — opening brace on its own line for all namespaces, types, and methods.

**Import order**: `System.*` → third-party (`Cysharp.*`, `MessagePipe`, `VContainer.*`) → Unity (`UnityEngine`, `UnityEditor`) → project (`VGameKit.*`). Remove unused directives (analyzers enforce this).

**Naming**:
- Types/Enums/Interfaces: `PascalCase`. Interface prefix: `I` (`IMenuPresenter`).
- Private/protected fields: `_camelCase`. Serialized: `[SerializeField] private _camelCase`. Methods/Properties: `PascalCase`.
- ScriptableObject auto-properties may use `[field: SerializeField]`. Constants: `PascalCase`.

**Formatting**: 4 spaces, no tabs. One blank line between members; no trailing whitespace. Expression-bodied members only when clarity improves. Prefer `var` when the RHS makes the type obvious.

**Generics & docs**: Add constraints when bounded (`where TMenuName : Enum`). `<typeparam>` XML on all public generics. `/// <summary>` on every `public`/`protected` member; private only when intent is non-obvious.

**Nullability**: `#nullable enable` absent project-wide. Guard with `if (x == null) return;`; prefer early returns.

**Async**: `UniTask`/`UniTaskVoid` only — never `System.Threading.Tasks.Task`. Pass `CancellationToken` through every async chain. Never block with `.GetAwaiter().GetResult()`. Use `.AttachExternalCancellation(token).SuppressCancellationThrow()` where appropriate.

**Error handling**: Throw only for programmer errors. Log and degrade gracefully for runtime failures. Wrap third-party callbacks in `try/catch` + `GKLog.Log(LogState.Error, ...)`.

**Logging**: `GKLog.Log` is `[Conditional("GAMEKIT_LOG")]` — stripped at compile time without the symbol.
`GKLog.Log(LogState.<level>, message)`. Full `LogState` flags: `Core`, `Development`, `Info`, `Analytics`, `IAP`, `Ads`, `Warning`, `Game`, `Pause`, `ProcessFlow`, `Error`, `Fatal`, `Booster`, `Timer`. Guard editor-only calls with `#if UNITY_EDITOR`.

## Dependency Injection

**Registration patterns**:
```csharp
// Plain-C# singleton:
builder.Register<MyManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

// Scene MonoBehaviour — always [SerializeField] field, never RegisterComponentInHierarchy:
[SerializeField] private MyMonoBehaviour _myMB;
builder.RegisterComponent(_myMB);

// Presenter:
builder.Register<MyPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

// Process flow — ProcessFlowProvider MUST be registered explicitly;
// RegisterProcessFlow calls container.Resolve<ProcessFlowProvider>() at runtime:
builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
builder.RegisterProcessFlow<TArgs, TFlow>(Lifetime.Singleton);
```
`AsImplementedInterfaces()` is required for VContainer to call `IInitializable.Initialize()` and `IDisposable.Dispose()`. Add `.AsSelf()` when the concrete type is also injected directly.

**Scopes**: App-level → `AbsMainLifetimeScope` (calls `RegisterMessagePipe()` internally — **never call it again** in subclasses). Scene/subsystem → `AbsBaseLifetimeScope`.

**Subscribable bases**:
- Plain-C#: inherit `SubscribableConcrete`; override `virtual Init()` and `Subscriptions()`.
- MonoBehaviour: inherit `SubscribableMonoBehaviour`; VContainer injects via `[Inject] void Construct(...)`, then calls `Initialize()`.
- **Never call `Initialize()` manually** on either base.
- `AbsAppManager` is concrete/sealed on `StartAsync` — override only `InitializeGame(CancellationToken)` and `OnAppReady(AppReadyEvent)`.

**Process flows** (canonical pattern — see `Assets/Demo/Runtime/ProcessExample.cs`):
```csharp
[Inject] private readonly Func<LoadLevelFlowArgs, LoadLevelFlow> _loadLevelFlow;
var flow = _loadLevelFlow(new LoadLevelFlowArgs(index));
flow.OnComplete(f => { /* ... */ });
flow.Execute(_cts.Token);  // caller owns the token — never flow.cancellationToken
```
Args with data: implement `IProcessFlowArgs` directly as a class. `BaseProcessFlowArgs` is a **struct** — cannot be inherited.

**MessagePipe**: `IPublisher<T>` / `ISubscriber<T>` resolve automatically. Always `AddTo(_bagBuilder)` on subscriptions.

## UI Systems
- **Menu**: `BaseMenuManager<TMenuName>` (MonoBehaviour) + `BaseMenuPresenter<TEnum, TData, TView>`. Enum identifiers only — no raw strings. `MenuMode.Single` auto-closes others.
- **Navigation**: publish `BaseOpenMenuEvent` / `BaseCloseMenuEvent` subclasses; never call `OpenMenu` from external callers.
- **Popup**: `BasePopupModel<TPopupName>` (enum type param required). Show via `_builder.AddPopup(Name.X, model).OpenPopup()`. Views extend `BasePopup<TPopupName>`; access model via `_model` cast; hooks: `OnShowBefore()`, `OnShowAfter()`, `OnHidePopup()`.
- **CloseOthersEvent**: `BaseCloseOthersMenuEvent<T>` constructor body is **empty** — it does not store `keepMenuNames`. Define `KeepMenuNames` as a property in the concrete subclass and assign it in the constructor.

## Spawner & Pooling
`BaseSpawnPool<TModel, TItem>` — inject `Func<TModel, Transform, TItem>` (stored as `_spawnFunc`). Fields: `_spawnFunc`, `_poolTarget` (Transform), `_inactiveItems` (Queue), `_activeItems` (List). `Dispose()` calls `RemoveAllObjects()` which destroys all GameObjects — do not substitute `HideAllObjects()`.

## IO & Config
Environment values → `GKConfig` ScriptableObjects. JSON persistence → `JSonKit`. Mark Unity fields `[SerializeField] private`; avoid public fields on non-POCO types.

## Known Bugs (Do Not Propagate)
1. **`MatrixId.!=`** — body identical to `==`; returns `true` when IDs are equal. Fix: negate.
2. **`AdsIds` Android non-test** — references undeclared `drd*Id` fields (`android*Id` are the real ones); compile error without `GOOGLEADS_TESTDEVICE`.
3. **`GoogleMobileAdsConsentController.GatherConsent`** — `WaitUntil` predicate `() => _initializeResult == None` is inverted; should be `!= None`.
4. **`JSonKit.ReadAllTextOnAndroid`** — deprecated `WWW` + main-thread busy-wait; replace with `UnityWebRequest` + UniTask.
5. **`RewardedAds.ResponseOpened`** — `onResponseAdEvent` only fires inside `#if GA_ENABLED`; silently dropped otherwise.
6. **`RewardedAds.RemoveListeners`** — nulls the entire static delegate, wiping all external subscribers.

## Documentation
Edit `docs/en/*` and `docs/tr/*` together. `python3 scripts/publish_wiki.py --dry-run` must output **63 pages, 0 errors**. Record new modules in `README.md` and `docs/PLAN.md`. Document every new public API, DI binding, or menu/spawner contract in both language folders.

## Pre-PR Checklist
1. `dotnet build VGameKit.sln` — zero warnings.
2. `dotnet format VGameKit.sln --verify-no-changes` — no drift.
3. Unity Play Mode smoke test in `SampleScene.unity`; confirm `GKLog` output.
4. Docs updated (EN + TR) for any public API or DI change.
5. Screenshots/GIFs for UI-facing changes.

## Git & Agent Tips
- Never commit `Library/`, `Temp/`, `Logs/`, user settings, `VGameKit.sln`, or `*.csproj`.
- Branch naming: `feature/<topic>`. Legacy typo branch `documenatation` exists — do not replicate.
- Stage Unity serialized-file changes deliberately (`git add -p`).
- Prefer `Read`, `Glob`, `Grep` tools over shell one-liners; Unity repos are sensitive to incidental changes.
- Keep diffs focused; log async/flow entry and exit points for future traceability.
