# VGameKit Agent Handbook

This playbook keeps autonomous agents aligned when working inside `/Volumes/Work/Personal/Projects/VGameKit`. Assume Unity quirks and keep instructions tight—Unity projects punish improvisation.

## Project Snapshot
1. **Engine**: Unity 6000.3.7f1 (`ProjectSettings/ProjectVersion.txt`).
2. **Languages**: C# 9 assemblies targeting `v4.7.1` (Unity Mono) via asmdefs under `Assets/VGameKit*`.
3. **Key deps**: VContainer 1.17.0 (DI), MessagePipe 1.8.1 (pub/sub), UniTask 2.5.10 (async), Cysharp GameAnalytics & Google Ads bridges.
4. **IDE defaults**: `.vscode/settings.json` hides `Library`, `Logs`, `Temp`, etc.—respect them.
5. **Cursor/Copilot rules**: none exist (`.cursor/**`, `.cursorrules`, `.github/copilot-instructions.md` absent); this file defines policy.
6. **Known compile symbols**: `GAMEKIT_LOG` (enables `GKLog` output), `GOOGLEADS_TESTDEVICE` (forces test device mode), `GA_ENABLED` (activates GameAnalytics paths).

## Repository Layout
- `Assets/VGameKit/**`: Core runtime (App, Config, Core, ProcessFlows, UI, Utilities, Spawner).
- `Assets/VGameKit.GA/**`: GameAnalytics integration glue.
- `Assets/VGameKit.GoogleAds/**`: Ads controllers, consent, editor tooling.
- `Assets/VGameKit.IO/**`: Serialization helpers (e.g., `JSonKit`).
- `Assets/Demo/**`: Example lifetime scopes and scenes (`Assets/Demo/Scenes/SampleScene.unity`).
- `docs/**`: Bilingual Diátaxis docs (sync `docs/en` and `docs/tr`).
- `ProjectSettings/`, `Packages/`, `UserSettings/`: Unity-managed; do not hand-edit.

## Tooling & MCP
1. Install Unity 6000.3.7f1 through Unity Hub; CLI path `/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app`.
2. Install .NET 8 SDK (latest LTS acceptable) for `dotnet build/format/test` on `VGameKit.slnx`.
3. MCP: `opencode.json` registers `unity-api` via `uvx unity-api-mcp` (env: `UNITY_PROJECT_PATH` set to the project root). Use `search_unity_api`, `get_method_signature`, etc., before writing unfamiliar APIs. Cache lives in `~/.unity-api-mcp/`; never commit it.

## Local Setup Checklist
1. Clone with LFS if you expect large binaries (`git lfs install`).
2. Run the project once in Unity so it regenerates `.csproj` files for CLI builds.
3. If `dotnet build` complains about workloads, execute `dotnet workload restore`.
4. Do not disable analyzer warnings; treat them as errors.

## Build Commands
1. **Unity player build (macOS example)**
   ```bash
   /Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity \
     -batchmode -quit \
     -projectPath "$PWD" \
     -buildTarget StandaloneOSX \
     -customBuildName VGameKitDemo \
     -logFile Logs/Editor-build.log
   ```
   - Outputs to `Builds/` (adjust via build scripts).
2. **Managed assemblies without Unity Editor**
   ```bash
   dotnet build VGameKit.slnx -c Debug
   ```
   - Use for CI smoke checks; requires up-to-date Unity-generated csproj files.
3. **CI-friendly asset refresh**: prefer Unity CLI re-imports; never delete `Library/` inside a shared worktree.

## Linting & Static Analysis
1. Unity + Microsoft analyzers run during `dotnet build`; treat warnings as failures.
2. Formatting pass (optional but recommended pre-PR):
   ```bash
   dotnet format VGameKit.slnx --verify-no-changes
   ```
   - Drop `--verify-no-changes` to apply fixes locally.
3. Only trigger `Assets > Reimport All` via Editor/CLI when absolutely needed; document the reason since it touches thousands of assets.

## Test Strategy & Commands
The repo currently ships demo scenes but no formal EditMode/PlayMode test assemblies. When tests exist, prefer Unity CLI over manual runs.

1. **All edit-mode tests**
   ```bash
   /Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity \
     -batchmode -quit \
     -projectPath "$PWD" \
     -runTests -testPlatform editmode \
     -logFile Logs/Editor-editmode.log
   ```
2. **All play-mode tests**
   ```bash
   /Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity \
     -batchmode -quit \
     -projectPath "$PWD" \
     -runTests -testPlatform playmode \
     -logFile Logs/Editor-playmode.log
   ```
3. **Single test filter (class or method)**
   ```bash
   /Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity \
     -batchmode -quit \
     -projectPath "$PWD" \
     -runTests -testPlatform playmode \
     -testFilter DemoFlowTests.Flow_executes \
     -logFile Logs/Editor-single.log
   ```
   - Update `-testPlatform`/`-testFilter` as needed; ensure names are unique to avoid broad matches.
4. **Demo scene smoke test**: open `Assets/Demo/Scenes/SampleScene.unity`, press Play, confirm `GKLog` output.

## Diagnostics & Logging
- Use `GKLog.Log(LogState, message)`; requires `GAMEKIT_LOG` compile symbol to emit output.
- Prefer `LogState.Development` for verbose debug, `LogState.Game` for user-impacting events, `LogState.Error` for failures.
- Guard async flows with start/end logs to trace UniTask chains.
- Wrap external SDK callbacks; log consent state transitions for Ads/GA interactions.

## Dependency Injection & Messaging
1. App-level scopes derive from `AbsMainLifetimeScope`; scene/subsystem scopes derive from `AbsBaseLifetimeScope`. Register builders, presenters, and pools via DI, never via `new` outside scopes.
2. `AbsMainLifetimeScope` calls `builder.RegisterMessagePipe()` internally and stores the returned options in `_messagePipeOpts`. Do **not** call `RegisterMessagePipe()` again in subclasses; use `_messagePipeOpts` for additional broker registrations.
3. `AbsAppManager` inherits `SubscribableConcrete` and implements `IAsyncStartable`—VContainer calls `StartAsync(CancellationToken)` automatically; do not call it manually.
4. For subscribable plain-C# types, inherit `SubscribableConcrete` and override the `virtual` methods `Init()` and `Subscriptions()`. VContainer calls `Initialize()` via `IInitializable`—do not call it manually.
5. For subscribable MonoBehaviours, inherit `SubscribableMonoBehaviour` (concrete class) and override the same `virtual` methods. VContainer injects via `[Inject] Construct()`—do not call it manually.
6. MessagePipe usage: inject `ISubscriber<T>`/`IPublisher<T>` with `[Inject]`; always `AddTo(_bagBuilder)` to avoid leaks.
7. Process flows: extend `BaseProcessFlow<TArgs>` (not `IFlowTask<T>`/`IFlowAsyncTask<T>` directly) and wire through `ProcessFlowProvider` so cancellation tokens propagate correctly.

## UI Systems
1. Menu managers: `BaseMenuManager<TMenuName>` plus presenters; enforce enum identifiers (`where TMenuName : Enum`, no raw strings). `MenuMode.Single` calls `CloseOthers()` automatically.
2. Popup builder: chain `AddPopup`, provide strongly typed `BasePopupModel` derivatives, finish with `.OpenPopup()`, optionally attach `.OnCompleteFlow`.
3. Ads UI: `AdsBaseView` plus Banner/Interstitial/Rewarded presenters; ensure consent via `GoogleMobileAdsConsentController` before personalized ads.

## Spawner & Pooling Patterns
- Implement pools via `BaseSpawnPool<TModel, TItem>`; inject `Func<TModel, Transform, TItem>` for DI-friendly instantiation.
- Always call `Dispose()` on pools when scope ends.
- Prefer reinitialization hooks on spawn items over re-creating GameObjects.

## IO, Config, and Serialization
- Store environment-specific values in `GKConfig` ScriptableObjects, not scripts.
- Use `JSonKit` for JSON persistence to keep encoding/casing consistent.
- Mark Unity fields `[SerializeField] private` (or protected) even when used internally; avoid public fields unless data objects require it.

## Code Style Guidelines
1. **Namespaces & braces**: follow Unity defaults—opening brace on new line for namespaces, types, and methods. Project namespaces follow the pattern `VGameKit.Runtime.*`, `VGameKit.IO.Runtime`, `VGameKit.GA.Runtime`, `VGameKit.GoogleAds.Runtime`.
2. **Imports**: order `System*`, third-party (Cysharp, MessagePipe, Unity), then project namespaces; remove unused directives (analyzers enforce).
3. **Formatting**: four spaces, no tabs; braces on new lines except auto-properties; use expression-bodied members only when clarity improves.
4. **Types & naming**: classes/interfaces/enums `PascalCase`; interfaces prefixed with `I`; private fields `_camelCase`; serialized fields `[SerializeField] private _camelCase`; ScriptableObject properties may use `[field: SerializeField]`; constants `PascalCase` unless `static readonly`.
5. **Generics**: add constraints (e.g., `where T : Enum` in `BaseMenuManager`); prefer `<typeparam>` XML docs on public generics.
6. **Nullability**: project does not use nullable references; guard manually (`if (_menuRoot is not null)`); prefer early returns.
7. **Async**: use `UniTask`/`UniTaskVoid` (only inside Unity context) and pass `CancellationToken` through; never block on `UniTask`.
8. **Error handling**: reserve exceptions for programmer errors; log and gracefully degrade for runtime issues (SDK init, consent, remote config). Wrap third-party calls in try/catch plus `GKLog` entries.
9. **Logging level**: push catastrophic errors to `LogState.Error`; keep debug noise under `LogState.Development`.
10. **Serialization**: avoid dynamic `GetComponent` during serialization callbacks; keep data POCOs plain.

## Known Bugs (Do Not Propagate)
The following defects exist in the current codebase. Do not copy these patterns when adding new code:

1. **`MatrixId.!=`**: operator body is identical to `==`—always returns `true` when components are equal. Fix by negating the `==` result.
2. **`AdsIds` (Android, non-test)**: references `drdBannerId`, `drdInterstitialId`, `drdRewardedId` fields that do not exist—causes a compile error when `GOOGLEADS_TESTDEVICE` is absent on Android.
3. **`GoogleMobileAdsConsentController.GatherConsent`**: race condition—`_initializeResult == None` predicate exits instantly before async init completes.
4. **`JSonKit.ReadAllTextOnAndroid`**: uses deprecated `WWW` with a busy-wait loop on the main thread; replace with `UnityWebRequest` + UniTask.
5. **`RewardedAds.ResponseOpened`**: callback only fires inside `#if GA_ENABLED`—silently drops events when GameAnalytics is disabled.
6. **`RewardedAds.RemoveListeners`**: nulls the entire static action delegate, clearing subscribers registered by external callers.

## Documentation Expectations
- Update `docs/en/*` and `docs/tr/*` together, mirroring the Diátaxis layout (`docs/PLAN.md`).
- Record new modules in README and `docs/PLAN.md`.
- Mention any new public API, DI binding, or menu/spawner contract in both language folders.

## Git & Workflow Tips
- Never commit `Library/`, `Temp/`, `Logs/`, or user-specific settings; `.gitignore` already covers them.
- Branch naming: prefer `feature/<topic>`; legacy typo `documenatation` exists—do not replicate.
- Unity assets create huge diffs; stage intentionally (`git add -p`) when editing serialized files.
- Do not revert user changes you did not author unless explicitly instructed.

## Validation Checklist Before PRs
1. Run `dotnet build VGameKit.slnx` and ensure zero warnings.
2. If runtime logic changed, perform a Unity Play Mode smoke test in `Assets/Demo/Scenes/SampleScene.unity`; capture relevant `GKLog` output.
3. Update docs for public API or DI changes.
4. Attach screenshots/gifs for UI-facing adjustments (menus, ads, popups).
5. Summarize Unity version bumps or package changes in the PR body.

## Agent Workflow Tips
- Prefer specialized tools (`Read`, `Glob`, `Grep`, `apply_patch`) over generic shell edits; Unity repos are sensitive to incidental changes.
- Never delete or regenerate Unity-managed folders by hand; rely on Unity CLI.
- When unsure about Unity APIs, consult the MCP `unity-api` server before coding.
- Keep diffs focused; avoid shotgun edits.
- Leave clear log statements around async/process flows to aid future investigation.

Stay disciplined—clear logs, deterministic builds, and consistent style let fellow agents pick up where you leave off without surprises.
