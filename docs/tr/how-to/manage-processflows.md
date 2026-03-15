# Process Flow'ları Nasıl Yönetilir

## Genel Bakış

Process flow'lar ayrı, iptal edilebilir, asenkron iş birimleridir. `BaseProcessFlow<TArgs>`, yürütme, zincirleme, iptal ve tamamlanma geri çağrımlarını yönetir. `ProcessFlowProvider`, çalışan tüm flow'ların yaşam döngüsünü yönetir.

**Ön Koşullar:** `VGameKit.Runtime` modülü kurulu; UniTask; bir `AbsBaseLifetimeScope`.

---

## Adım 1 — Flow argümanlarını tanımlayın

`IProcessFlowArgs` arayüzünü uygulayan bir class oluşturun. Argümanları constructor üzerinden iletin; böylece oluşturma anında immutable olurlar.

```csharp
using VGameKit.Runtime.ProcessFlows;

public class LoadLevelFlowArgs : IProcessFlowArgs
{
    public int LevelIndex { get; private set; }

    public LoadLevelFlowArgs(int levelIndex)
    {
        LevelIndex = levelIndex;
    }
}
```

`BaseProcessFlowArgs`, boş bir struct kolaylığıdır — yalnızca flow'un hiçbir girdiye ihtiyaç duymadığı durumlarda kullanın.

---

## Adım 2 — Flow'u uygulayın

`BaseProcessFlow<TArgs>`'dan kalıtım alan ve `AsyncExecute`'u uygulayan bir class oluşturun.

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.ProcessFlows;

public class LoadLevelFlow : BaseProcessFlow<LoadLevelFlowArgs>
{
    // Flow'un ihtiyaç duyduğu bağımlılıkları inject edin
    [Inject] private readonly LevelPrefabs _levelPrefabs;

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        var levelPrefab = _levelPrefabs.Levels[Args.LevelIndex];

        LevelObj level = null;
        await UniTask.WaitUntil(
            () => level = Object.Instantiate(levelPrefab).GetComponent<LevelObj>(),
            cancellationToken: ctx);

        level.Initialize();
        return this;
    }
}
```

`AsyncExecute`, işi bitirdikten sonra `this`'i döndürmelidir. `this` döndürmek tamamlanmayı ve devam zincirini tetikler.

`AsyncExecute` öncesinde senkron ön kurulum gerekiyorsa isteğe bağlı `protected virtual void Initialize()` metodunu override edin.

---

## Adım 3 — LifetimeScope'a kaydedin

`Func<TArgs, TFlow>` fabrikasını kaydetmek için `RegisterProcessFlow<TArgs, TFlow>` kullanın. `ProcessFlowProvider`'ı da singleton olarak kaydedin — `RegisterProcessFlow` dahili olarak `container.Resolve<ProcessFlowProvider>()` çağırır; provider container'da kayıtlı değilse çalışma zamanında çözümleme hatası fırlatır.

```csharp
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class AppLifetimeScope : AbsMainLifetimeScope
{
    [SerializeField] private LevelPrefabs _levelPrefabs;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // ProcessFlowProvider açıkça kaydedilmeli — RegisterProcessFlow ona bağımlıdır.
        // AsImplementedInterfaces(), VContainer'ın IDisposable.Dispose()'u otomatik çağırmasını sağlar.
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        builder.RegisterComponent(_levelPrefabs);

        // AsImplementedInterfaces(), VContainer'ın IInitializable.Initialize() ve
        // IDisposable.Dispose() çağırmasını sağlar. AsSelf() ise DemoManager'ın
        // somut tipiyle de çözümlenmesine olanak tanır.
        builder.Register<DemoManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        // Func<LoadLevelFlowArgs, LoadLevelFlow> kaydeder
        builder.RegisterProcessFlow<LoadLevelFlowArgs, LoadLevelFlow>(Lifetime.Singleton);
    }
}
```

`RegisterProcessFlow`, dahili olarak `builder.RegisterFactory` çağırır. Fabrika çağrıldığında `provider.CreateProcessFlow<TArgs, TFlow>(args)` metoduna delege eder.

---

## Adım 4 — Flow oluşturun ve çalıştırın

`Func<TArgs, TFlow>`'u doğrudan inject edin — flow oluşturma için `ProcessFlowProvider`'ı inject etmeyin. Çağıran sınıf bir `CancellationTokenSource` yönetir ve token'ı `Execute`'a geçirir. Token'ı `IInitializable.Initialize()`'da oluşturun ve `IDisposable.Dispose()`'da iptal edin:

```csharp
using System;
using System.Threading;
using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.ProcessFlows;

public class DemoManager : IInitializable, IDisposable
{
    [Inject] private readonly Func<LoadLevelFlowArgs, LoadLevelFlow> _loadLevelFlow;

    private CancellationTokenSource _cts;

    public void Initialize()
    {
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        _cts?.Cancel();
    }

    public void LoadLevel(int levelIndex)
    {
        var args = new LoadLevelFlowArgs(levelIndex);
        var flow = _loadLevelFlow(args);

        flow.OnComplete(f =>
        {
            if (f is LoadLevelFlow lf)
            {
                Debug.Log($"Seviye {lf.Args.LevelIndex} başarıyla yüklendi.");
            }
        });

        flow.Execute(_cts.Token);
    }
}
```

`_loadLevelFlow(args)` çağrısı `provider.CreateProcessFlow`'u tetikler: `new()` ile yeni örnek oluşturur, bağımlılıkları inject eder, `Initialize(args)`'ı çağırır ve flow'u tamamlandığında otomatik kaldırılmak üzere `AddProcessFlow` ile kaydeder.

---

## Adım 5 — Flow'ları zincirleyin

Flow'ları zincirlemenin iki yolu vardır. Seçim, sıralama sorumluluğunun nerede olduğuna göre yapılır.

### Seçenek A — `AppendProcess` (çağıran taraflı zincirleme)

Çağıran her iki flow'u da oluşturur ve çalıştırmadan önce birbirine bağlar. Eklenen flow'lar, eklenme sıralarına göre **sırayla** çalışır; bir önceki flow'un `AsyncExecute`'u `this`'i döndürdükten sonra `ContinuationFunction` tarafından otomatik çalıştırılırlar. Eklenen flow'un `Execute`'unu kendiniz çağırmayın.

```csharp
[Inject] private readonly Func<LoadLevelFlowArgs, LoadLevelFlow> _loadLevelFlow;
[Inject] private readonly Func<SpawnEnemiesArgs, SpawnEnemiesFlow> _spawnEnemiesFlow;

public void StartLevel(int index)
{
    var loadFlow  = _loadLevelFlow(new LoadLevelFlowArgs(index));
    var spawnFlow = _spawnEnemiesFlow(new SpawnEnemiesArgs());

    loadFlow.AppendProcess(spawnFlow);
    loadFlow.Execute(_cts.Token);
    // Çalışma sırası: loadFlow.AsyncExecute → spawnFlow.AsyncExecute
    // spawnFlow.Execute otomatik çağrılır — kendiniz çağırmayın
}
```

Aynı kök flow'a birden fazla flow eklenebilir; eklenme sırasıyla birer birer çalışırlar:

```csharp
rootFlow.AppendProcess(secondFlow);
rootFlow.AppendProcess(thirdFlow);
rootFlow.Execute(_cts.Token);
// Sıra: rootFlow → secondFlow → thirdFlow
```

### Seçenek B — `AsyncExecute` içinde satır içi zincirleme

Bir flow, kendi `AsyncExecute`'u içinden bir sonraki flow'u oluşturur ve çalıştırır. Sıralama, çağıranın değil flow'un kendi iç detayı olduğunda bu yöntemi kullanın.

```csharp
public class LoadLevelFlow : BaseProcessFlow<LoadLevelFlowArgs>
{
    [Inject] private readonly Func<SpawnEnemiesArgs, SpawnEnemiesFlow> _spawnEnemiesFlow;

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        // ... seviye yükleme işlemleri ...

        // Sonraki flow'u satır içinde başlat ve tamamlanmasını bekle
        var spawnFlow = _spawnEnemiesFlow(new SpawnEnemiesArgs());
        spawnFlow.Execute(ctx);
        await UniTask.WaitUntil(() => spawnFlow.IsCompleted, cancellationToken: ctx);

        return this;
    }
}
```

Çağıran yalnızca `LoadLevelFlow`'u oluşturur ve çalıştırır; `SpawnEnemiesFlow` onun içinde gizlidir. Devam flow'u her zaman ilk flow tarafından zorunlu kılınıyorsa ve bağımsız kullanımı yoksa bu deseni tercih edin.

**Hangisini ne zaman kullanmalı:**
- `AppendProcess` — sıralamaya çağıran karar verir; flow'lar bağımsız olarak yeniden kullanılabilir.
- Satır içi `AsyncExecute` — sıralama her zaman flow'un içinde sabittir; çağıranın bunu bilmesi gerekmez.

---

## Adım 6 — Tamamlanmaya tepki verin

`Execute`'dan önce bir geri çağrım kaydedin:

```csharp
flow.OnComplete(completedFlow =>
{
    Debug.Log("Seviye başarıyla yüklendi.");
});
flow.Execute(_cts.Token);
```

---

## Adım 7 — Flow'u iptal edin

```csharp
// Belirli bir tipi iptal et (yönetim işlemleri için ProcessFlowProvider inject edin):
[Inject] private readonly ProcessFlowProvider _flowProvider;

_flowProvider.RemoveProcessFlow<LoadLevelFlow>();

// Tüm çalışan flow'ları iptal et:
_flowProvider.RemoveAllProcessFlows();
```

`RemoveProcessFlow<T>` çağrısı token'ı iptal eder, flow'u dispose eder ve provider'ın dahili listesinden kaldırır.

---

## Flow yaşam döngüsü özeti

```
RegisterProcessFlow  →  Func<TArgs, TFlow> çağırana inject edilir
        ↓
  factory(args) çağrılır
        ↓
  provider.CreateProcessFlow:
    new TFlow()  →  Inject()  →  Initialize(args)  →  AddProcessFlow()
        ↓
  flow.Execute(_cts.Token)
        ↓
  AsyncExecute()
        ↓
  ContinuationFunction()
  (AppendProcess ile eklenen flow'lar eklenme sırasıyla birer birer çalışır)
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
