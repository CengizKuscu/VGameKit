# Process Flow'ları Nasıl Yönetilir

## Genel Bakış

Process flow'lar ayrı, iptal edilebilir, asenkron iş birimleridir. `BaseProcessFlow<TArgs>`, yürütme, zincirleme, iptal ve tamamlanma geri çağrımlarını yönetir. `ProcessFlowProvider`, çalışan tüm flow'ların yaşam döngüsünü yönetir.

**Ön Koşullar:** `VGameKit.Runtime` modülü kurulu; UniTask; bir `AbsBaseLifetimeScope`.

---

## Adım 1 — Flow argümanlarını tanımla

`IProcessFlowArgs`'ı uygulayan bir sınıf oluşturun. Flow'un çalışma zamanında ihtiyaç duyduğu veriyi taşır.

```csharp
using VGameKit.Runtime.ProcessFlows;

public class LoadLevelArgs : BaseProcessFlowArgs
{
    public int LevelIndex;
    public bool ShowLoadingScreen;
}
```

`BaseProcessFlowArgs`, zorunlu üyesi olmayan bir kolaylık temel sınıfıdır — doğrudan `IProcessFlowArgs`'ı da uygulayabilirsiniz.

---

## Adım 2 — Flow'u uygula

`BaseProcessFlow<TArgs>`'dan kalıtım alan ve `AsyncExecute`'u uygulayan bir sınıf oluşturun.

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class LoadLevelFlow : BaseProcessFlow<LoadLevelArgs>
{
    // Bu flow'un ihtiyaç duyduğu bağımlılıkları inject edin
    [Inject] private readonly SceneLoader _sceneLoader;

    protected override void Initialize()
    {
        GKLog.Log(LogState.ProcessFlow, $"LoadLevelFlow: Initialize level {Args.LevelIndex}");
    }

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        GKLog.Log(LogState.ProcessFlow, $"LoadLevelFlow: Loading level {Args.LevelIndex}");

        await _sceneLoader.LoadAsync(Args.LevelIndex, ctx);

        GKLog.Log(LogState.ProcessFlow, "LoadLevelFlow: Done");
        return this;
    }
}
```

`AsyncExecute`, işi bitirdikten sonra `this`'i (veya bir sonraki `IProcessFlow`'u) döndürmelidir. `this` döndürmek tamamlanmayı ve devam zincirini tetikler.

---

## Adım 3 — LifetimeScope'a kaydet

`ProcessFlowProvider`'ı singleton olarak kaydedin. Bireysel flow'lar provider içinde `new()` + `Inject()` ile oluşturulur — onları DI tipi olarak **kaydetmeyin**.

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameLifetimeScope : AbsBaseLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        builder.Register<ProcessFlowProvider>(Lifetime.Singleton);
    }
}
```

---

## Adım 4 — Flow oluştur ve çalıştır

`ProcessFlowProvider`'ı inject edin ve `CreateProcessFlow<TArgs, TFlow>` çağrısı yapın:

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameController : SubscribableConcrete
{
    [Inject] private readonly ProcessFlowProvider _flowProvider;

    public void StartLoadLevel(int levelIndex)
    {
        var args = new LoadLevelArgs
        {
            LevelIndex = levelIndex,
            ShowLoadingScreen = true
        };

        var flow = _flowProvider.CreateProcessFlow<LoadLevelArgs, LoadLevelFlow>(args);
        flow.Execute(flow.cancellationToken);
    }
}
```

`CreateProcessFlow`, tek çağrıda `new()`, `Inject()`, `Initialize(args)` ve `AddProcessFlow()` işlemlerini gerçekleştirir. Provider, flow tamamlandığında onu otomatik olarak kaldırır ve dispose eder.

---

## Adım 5 — Flow'ları zincirle

İkinci bir flow'u birinciden sonra çalıştırmak için `AppendProcess` kullanın:

```csharp
var loadFlow  = _flowProvider.CreateProcessFlow<LoadLevelArgs, LoadLevelFlow>(loadArgs);
var spawnFlow = _flowProvider.CreateProcessFlow<SpawnEnemiesArgs, SpawnEnemiesFlow>(spawnArgs);

loadFlow.AppendProcess(spawnFlow);
loadFlow.Execute(loadFlow.cancellationToken);
// loadFlow tamamlandıktan sonra spawnFlow.Execute otomatik çağrılır
```

---

## Adım 6 — Tamamlanmaya tepki ver

`Execute`'dan önce bir geri çağrım kaydedin:

```csharp
flow.OnComplete(completedFlow =>
{
    GKLog.Log(LogState.Game, "Seviye başarıyla yüklendi.");
});
flow.Execute(flow.cancellationToken);
```

---

## Adım 7 — Flow'u iptal et

```csharp
// Belirli bir tipi iptal et:
_flowProvider.RemoveProcessFlow<LoadLevelFlow>();

// Tüm çalışan flow'ları iptal et:
_flowProvider.RemoveAllProcessFlows();
```

`RemoveProcessFlow<T>` çağrısı token'ı iptal eder, flow'u dispose eder ve provider'ın dahili listesinden kaldırır.

---

## Flow yaşam döngüsü özeti

```
CreateProcessFlow  →  Initialize()  →  Execute()
      ↓                                    ↓
  AddProcessFlow                      AsyncExecute()
                                           ↓
                                    ContinuationFunction()
                                    (eklenmiş flow'lar burada çalışır)
                                           ↓
                                    onComplete geri çağrımları
                                           ↓
                                    provider otomatik kaldırır ve dispose eder
```

---

## Ayrıca Bkz.

- [ProcessFlows referansı](../reference/api/processflows.md)
- [Lifetime Scopes referansı](../reference/api/lifetime-scopes.md)
- [Loglama referansı](../reference/api/logging.md)
