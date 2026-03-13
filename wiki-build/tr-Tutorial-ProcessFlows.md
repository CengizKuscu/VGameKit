# Tutorial: Process Flow'ları Öğrenme

## Ne İnşa Edeceksiniz

Üç adımlı bir oyun başlatma sekansı — **kayıt doğrulama**, **asset yükleme**, **HUD gösterme** — her adım ayrı bir `BaseProcessFlow`, sırayla çalışıyorlar, her biri iptal edilebilir ve sonuç konsolda görüntüleniyor.

**Süre:** ~30 dakika  
**Ön Koşullar:** [Başlarken](tr-Tutorial-Getting-Started) tutorial'ını tamamladınız; `VGameKit.Runtime` kurulu.

---

## Adım 1 — Args sınıflarını oluşturun

Her flow kendi argüman tipini taşır.

```csharp
using VGameKit.Runtime.ProcessFlows;

public class ValidateSaveArgs : BaseProcessFlowArgs
{
    public string SaveSlot;
}

public class LoadAssetsArgs : BaseProcessFlowArgs
{
    public string[] BundleNames;
}

public class ShowHUDArgs : BaseProcessFlowArgs { }
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

## Adım 5 — ProcessFlowProvider'ı kaydedin

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.ProcessFlows;

public class GameLifetimeScope : AbsMainLifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.Register<ProcessFlowProvider>(Lifetime.Singleton);
        builder.Register<GameStarter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
    }
}
```

---

## Adım 6 — Flow'ları zincirleyin ve çalıştırın

```csharp
using VContainer;
using VGameKit.Runtime.Core;
using VGameKit.Runtime.Log;
using VGameKit.Runtime.ProcessFlows;

public class GameStarter : SubscribableConcrete
{
    [Inject] private readonly ProcessFlowProvider _flowProvider;

    protected override void Init()
    {
        StartSequence();
    }

    private void StartSequence()
    {
        var validateFlow = _flowProvider.CreateProcessFlow<ValidateSaveArgs, ValidateSaveFlow>(
            new ValidateSaveArgs { SaveSlot = "slot_0" });

        var loadFlow = _flowProvider.CreateProcessFlow<LoadAssetsArgs, LoadAssetsFlow>(
            new LoadAssetsArgs { BundleNames = new[] { "ui", "audio" } });

        var hudFlow = _flowProvider.CreateProcessFlow<ShowHUDArgs, ShowHUDFlow>(
            new ShowHUDArgs());

        // Zincirle: validate → load → hud
        validateFlow.AppendProcess(loadFlow);
        loadFlow.AppendProcess(hudFlow);

        hudFlow.OnComplete(_ =>
            GKLog.Log(LogState.Game, "GameStarter: başlangıç sekansı tamamlandı."));

        validateFlow.Execute(validateFlow.cancellationToken);
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

Bir iptali simüle etmek için (ör. oyuncu bağlantısı kesildi):

```csharp
_flowProvider.RemoveProcessFlow<LoadAssetsFlow>();
// Bekleyen tüm eklenmiş flow'lar da iptal edilir.
```

---

## Ne Öğrendiniz

- Karmaşık sekansları ayrı, tek sorumluluğa sahip flow'lara nasıl böleceğinizi.
- `AppendProcess`'in sıkı bağlantı olmadan flow'ları nasıl zincirlediğini.
- `OnComplete`'in flow'u değiştirmeden çağıranların tepki vermesini nasıl sağladığını.
- `RemoveProcessFlow<T>`'nin temiz sekans ortası iptali nasıl sağladığını.

---

## Sonraki Adımlar

- [Nasıl yapılır: Process flow'ları yönet](tr-HowTo-Manage-ProcessFlows)
- [Referans: ProcessFlows](tr-Reference-ProcessFlows)
- [Referans: Loglama](tr-Reference-Logging)
