# Tutorial: Process Flow'ları Öğrenme

## Ne İnşa Edeceksiniz

Üç adımlı bir oyun başlatma sekansı — **kayıt doğrulama**, **asset yükleme**, **HUD gösterme** — her adım ayrı bir `BaseProcessFlow`, sırayla çalışıyorlar, her biri iptal edilebilir ve sonuç konsolda görüntüleniyor.

**Süre:** ~30 dakika  
**Ön Koşullar:** [Başlarken](./getting-started.md) tutorial'ını tamamladınız; `VGameKit.Runtime` kurulu.

---

## Adım 1 — Args sınıflarını oluşturun

Her flow kendi argüman tipini taşır.

```csharp
using VGameKit.Runtime.ProcessFlows;

// IProcessFlowArgs arayüzünü doğrudan implement edin — BaseProcessFlowArgs bir struct'tır
// ve kalıtım alınamaz. Yalnızca girdi gerektirmeyen flow'larda typed boş değer olarak kullanın.
public class ValidateSaveArgs : IProcessFlowArgs
{
    public string SaveSlot;
}

public class LoadAssetsArgs : IProcessFlowArgs
{
    public string[] BundleNames;
}

public class ShowHUDArgs : IProcessFlowArgs { }
```

---

## Adım 2 — ValidateSaveFlow'u uygulayın

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class ValidateSaveFlow : BaseProcessFlow<ValidateSaveArgs>
{
    protected override void Initialize()
    {
        GKLog.Log(LogState.Development, $"ValidateSaveFlow: '{Args.SaveSlot}' slotu kontrol ediliyor");
    }

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        await UniTask.Delay(100, cancellationToken: ctx); // I/O simülasyonu
        GKLog.Log(LogState.Game, "ValidateSaveFlow: kayıt geçerli.");
        return this;
    }
}
```

---

## Adım 3 — LoadAssetsFlow'u uygulayın

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class LoadAssetsFlow : BaseProcessFlow<LoadAssetsArgs>
{
    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        foreach (var bundle in Args.BundleNames)
        {
            GKLog.Log(LogState.Development, $"LoadAssetsFlow: '{bundle}' yükleniyor");
            await UniTask.Delay(150, cancellationToken: ctx);
        }
        GKLog.Log(LogState.Game, "LoadAssetsFlow: tüm asset'ler yüklendi.");
        return this;
    }
}
```

---

## Adım 4 — ShowHUDFlow'u uygulayın

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class ShowHUDFlow : BaseProcessFlow<ShowHUDArgs>
{
    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        GKLog.Log(LogState.Game, "ShowHUDFlow: HUD gösterildi.");
        await UniTask.CompletedTask;
        return this;
    }
}
```

---

## Adım 5 — Flow'ları LifetimeScope'a kaydedin

Her flow için `Func<TArgs, TFlow>` fabrikası kaydetmek amacıyla `RegisterProcessFlow<TArgs, TFlow>` kullanın. `ProcessFlowProvider`'ın da açıkça kaydedilmesi zorunludur — `RegisterProcessFlow` dahili olarak `container.Resolve<ProcessFlowProvider>()` çağırır; provider container'da yoksa çalışma zamanında hata alınır.

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        // ProcessFlowProvider önce kaydedilmeli — RegisterProcessFlow ona bağımlıdır.
        // AsImplementedInterfaces(), VContainer'ın IDisposable.Dispose()'u otomatik çağırmasını sağlar.
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        // Her çağrı bir Func<TArgs, TFlow> fabrikası kaydeder
        builder.RegisterProcessFlow<ValidateSaveArgs, ValidateSaveFlow>(Lifetime.Singleton);
        builder.RegisterProcessFlow<LoadAssetsArgs, LoadAssetsFlow>(Lifetime.Singleton);
        builder.RegisterProcessFlow<ShowHUDArgs, ShowHUDFlow>(Lifetime.Singleton);

        builder.Register<GameStarter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Adım 6 — Flow'ları zincirleyin ve çalıştırın

`Func<TArgs, TFlow>` fabrikalarını doğrudan inject edin — flow oluşturma için `ProcessFlowProvider`'ı inject etmeyin. Çağıran sınıf bir `CancellationTokenSource` yönetir ve token'ı `Execute`'a geçirir. `GameStarter`, `SubscribableConcrete`'i genişlettiğinden `_cts`'yi `Init()`'te oluşturun ve `Dispose()`'u override ederek iptal edin:

```csharp
using System;
using System.Threading;
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class GameStarter : SubscribableConcrete
{
    [Inject] private readonly Func<ValidateSaveArgs, ValidateSaveFlow> _validateFlow;
    [Inject] private readonly Func<LoadAssetsArgs, LoadAssetsFlow> _loadFlow;
    [Inject] private readonly Func<ShowHUDArgs, ShowHUDFlow> _hudFlow;

    private CancellationTokenSource _cts;

    protected override void Init()
    {
        _cts = new CancellationTokenSource();
        StartSequence();
    }

    public override void Dispose()
    {
        _cts?.Cancel();
        base.Dispose();
    }

    private void StartSequence()
    {
        var validateFlow = _validateFlow(new ValidateSaveArgs { SaveSlot = "slot_0" });
        var loadFlow     = _loadFlow(new LoadAssetsArgs { BundleNames = new[] { "ui", "audio" } });
        var hudFlow      = _hudFlow(new ShowHUDArgs());

        // Zincirle: validate → load → hud
        validateFlow.AppendProcess(loadFlow);
        loadFlow.AppendProcess(hudFlow);

        hudFlow.OnComplete(_ =>
            GKLog.Log(LogState.Game, "GameStarter: başlangıç sekansı tamamlandı."));

        validateFlow.Execute(_cts.Token);
    }
}
```

---

## Adım 7 — Çalıştırın ve gözlemleyin

Play Mode'a girin. Konsol çıktısı sırayla görünmeli:

```
[Development] ValidateSaveFlow: 'slot_0' slotu kontrol ediliyor
[Game]        ValidateSaveFlow: kayıt geçerli.
[Development] LoadAssetsFlow: 'ui' yükleniyor
[Development] LoadAssetsFlow: 'audio' yükleniyor
[Game]        LoadAssetsFlow: tüm asset'ler yüklendi.
[Game]        ShowHUDFlow: HUD gösterildi.
[Game]        GameStarter: başlangıç sekansı tamamlandı.
```

---

## Adım 8 — Sekans ortasında iptal edin

`GameStarter`'da mevcut olan `CancellationTokenSource` üzerinden `_cts.Cancel()` çağrısı yaparak tüm zinciri anında iptal edebilirsiniz:

```csharp
// Seçenek A — CancellationTokenSource üzerinden tüm zinciri iptal et (GameStarter'da mevcut):
_cts.Cancel();
// Zincirdeki tüm flow'lar eş zamanlı iptal sinyali alır.
```

Yalnızca belirli bir flow tipini iptal etmek için `ProcessFlowProvider`'ı ayrı bir yönetici sınıfa inject edin:

```csharp
// Seçenek B — ProcessFlowProvider üzerinden belirli bir flow tipini iptal et:
public class GameController : IInitializable, IDisposable
{
    [Inject] private readonly ProcessFlowProvider _flowProvider;

    public void Initialize() { }
    public void Dispose() { }

    public void AbortLoading()
    {
        _flowProvider.RemoveProcessFlow<LoadAssetsFlow>();
        // Bekleyen tüm eklenmiş flow'lar da iptal edilir.
    }
}
```

---

## Ne Öğrendiniz

- Karmaşık sekansları ayrı, tek sorumluluğa sahip flow'lara nasıl böleceğinizi.
- `AppendProcess`'in sıkı bağlantı olmadan flow'ları nasıl zincirlediğini.
- `OnComplete`'in flow'u değiştirmeden çağıranların tepki vermesini nasıl sağladığını.
- `CancellationTokenSource`'un tüm zinciri nasıl iptal ettiğini; `RemoveProcessFlow<T>`'nin `ProcessFlowProvider` üzerinden belirli bir flow tipini nasıl iptal ettiğini.

---

## Sonraki Adımlar

- [Nasıl yapılır: Process flow'ları yönet](../how-to/manage-processflows.md)
- [Referans: ProcessFlows](../reference/api/processflows.md)
- [Referans: Loglama](../reference/api/logging.md)
