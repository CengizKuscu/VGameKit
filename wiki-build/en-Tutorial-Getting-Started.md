# Tutorial: Getting Started with VGameKit

## What you will build

By the end of this tutorial you will have a minimal Unity scene with a VGameKit `AppManager` running, producing log output, and ready to host further VGameKit systems.

**Time:** ~20 minutes  
**Prerequisites:** Unity 6000.3.7f1 installed; basic C# and Unity editor knowledge; VContainer and UniTask packages available.

---

## Step 1 — Create a new Unity project

1. Open Unity Hub and create a **3D (Core)** project targeting Unity 6000.3.7f1.
2. Open **Window > Package Manager** and install:
   - **VContainer** (via Git URL or OpenUPM)
   - **UniTask** (Cysharp, via Git URL)
   - **MessagePipe** (Cysharp, via Git URL)

Install VGameKit modules via **Package Manager > + > Add package from Git URL**, or add them to the `dependencies` block in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.cngz.vgamekit": "https://github.com/CengizKuscu/VGameKit.git?path=Assets/VGameKit#v0.0.4"
  }
}
```

For JSON persistence, ads, or analytics support add the optional modules as well — see [How to Install VGameKit Modules](en-HowTo-Install-Modules) for the full list of Git URLs.

---

## Step 2 — Enable the GAMEKIT_LOG compile symbol

VGameKit's `GKLog` only emits output when `GAMEKIT_LOG` is defined.

1. Go to **Edit > Project Settings > Player > Scripting Define Symbols**.
2. Add `GAMEKIT_LOG` and click **Apply**.

You should now see `GKLog` calls in the console during Play Mode.

---

## Step 3 — Create the App LifetimeScope

Create a new C# class `MyAppLifetimeScope` inheriting from `AbsMainLifetimeScope`.

```csharp
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Core;

public class MyAppLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.Register<MyAppManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

`base.Configure` registers MessagePipe and the VGameKit core services. Do not call `builder.RegisterMessagePipe()` yourself.

---

## Step 4 — Create the App Manager

Create `MyAppManager` inheriting from `AbsAppManager`:

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.App;
using VGameKit.Runtime.Log;

public class MyAppManager : AbsAppManager
{
    public override async UniTask StartAsync(CancellationToken cancellation)
    {
        GKLog.Log(LogState.Game, "MyAppManager: App started.");
        await UniTask.CompletedTask;
    }
}
```

VContainer calls `StartAsync` automatically via `IAsyncStartable` — do not call it manually.

---

## Step 5 — Wire the scene

1. Open the default scene (`SampleScene` or create one).
2. Create an empty GameObject named **AppScope**.
3. Attach `MyAppLifetimeScope` to it.
4. Enter Play Mode.

You should see in the Console:

```
[Game] MyAppManager: App started.
```

---

## Step 6 — Add a config asset

VGameKit stores environment values in `GKConfig` ScriptableObjects.

1. Right-click in the Project window → **Create > VGameKit > Config**.
2. Name it `AppConfig` and set any fields you need.
3. Assign it to a field in `MyAppLifetimeScope` or read it via DI.

---

## What you learned

- How to bootstrap VGameKit's DI container with `AbsMainLifetimeScope`.
- How `AbsAppManager` / `IAsyncStartable` provides a safe async entry point.
- How to enable `GKLog` output with a compile symbol.
- How `GKConfig` ScriptableObjects hold environment-specific data.

---

## Next steps

- [Tutorial: Building your first mini-game](en-Tutorial-First-Game)
- [How-to: Configure logging](en-HowTo-Configure-Logging)
- [Reference: Lifetime Scopes](en-Reference-Lifetime-Scopes)
