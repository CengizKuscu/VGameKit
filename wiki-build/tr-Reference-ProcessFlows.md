# ProcessFlows Referansı

**Ad Alanı:** `VGameKit.Runtime.ProcessFlows`
**Derleme:** `VGameKit.Runtime`
**Dosyalar:** `Assets/VGameKit/Runtime/ProcessFlows/`

---

## Genel Bakış

ProcessFlows sistemi, asenkron oyun mantığını sıralamak için yapılandırılmış bir ardışık düzen (pipeline) sunar. Bir "process flow" şunlara sahip ayrık ve iptal edilebilir bir iş birimidir:
- Tiplendirilmiş argümanlar alır (`IProcessFlowArgs`)
- Asenkron mantığı yürütür (`AsyncExecute`)
- `AppendProcess` ile ek flow'lar zincirleyebilir
- Tamamlanma geri çağrısı (callback) tetikler (`onComplete`)
- `ProcessFlowProvider` tarafından merkezi olarak yönetilir

---

## Tip Özeti

| Tip | Tür | Açıklama |
|---|---|---|
| `IProcessFlowArgs` | arayüz | Flow argüman kümesi için işaretçi |
| `BaseProcessFlowArgs` | struct | Boş varsayılan argümanlar (argüman gerekmediğinde) |
| `IFlowTask<T>` | arayüz | Senkron, kendini döndüren görev |
| `IFlowAsyncTask<T>` | arayüz | Asenkron, kendini döndüren görev |
| `IProcessFlow` | arayüz | Tam flow sözleşmesi |
| `IProcessFlow<TArgs>` | arayüz | Argüman erişimi olan tiplendirilmiş flow |
| `BaseProcessFlow<TArgs>` | soyut sınıf | Temel uygulama |
| `ProcessFlowProvider` | sınıf | Merkezi kayıt ve fabrika |
| `ProcessFlowsExtensions` | statik sınıf | DI kayıt yardımcısı |

---

## Arayüzler

### `IProcessFlowArgs`

```csharp
public interface IProcessFlowArgs { }
```

İşaretçi arayüzü. Tiplendirilmiş girdi gerektiren her flow için bu arayüzü uygulayan bir struct veya class oluşturun.

---

### `BaseProcessFlowArgs`

```csharp
public struct BaseProcessFlowArgs : IProcessFlowArgs { }
```

Boş argüman struct'ı — flow'un giriş parametresi gerektirmediği durumlarda kullanın.

---

### `IFlowTask<T>`

```csharp
public interface IFlowTask<T> where T : IFlowTask<T>
{
    T Execute();
}
```

Senkron, kendini döndüren görev. Asenkron olmayan ardışık düzen adımları için uygulayın.

---

### `IFlowAsyncTask<T>`

```csharp
public interface IFlowAsyncTask<T> where T : IFlowAsyncTask<T>
{
    UniTask<T> Execute(CancellationToken ctx = default);
}
```

Asenkron, kendini döndüren görev. Tam `IProcessFlow` sistemi dışındaki asenkron adımlar için uygulayın.

---

### `IProcessFlow`

```csharp
public interface IProcessFlow : IDisposable
```

| Üye | Tip | Açıklama |
|---|---|---|
| `onComplete` | `Action<IProcessFlow>` | Tüm flow'lar (eklenmiş olanlar dahil) tamamlandığında tetiklenir |
| `Id` | `string` | Benzersiz kimlik (varsayılan olarak tip adı) |
| `IsCompleted` | `bool` | Tüm zincirleme flow'lar tamamlandıktan sonra `true` |
| `cancellationTokenSource` | `CancellationTokenSource` | Geç oluşturulur; iptal için kullanın |
| `cancellationToken` | `CancellationToken` | `=> cancellationTokenSource.Token` kısayolu |
| `Initialize(IProcessFlowArgs)` | metot | Argümanları ayarlar ve korunan `Initialize()`'ı çağırır |
| `AppendProcess(IProcessFlow)` | metot | Bu flow tamamlandıktan sonra çalışacak bir flow zincirler |
| `OnComplete(Action<IProcessFlow>)` | metot | Geri çağrı ekler; zincirleme için `this` döndürür |
| `Execute(CancellationToken)` | metot | "Ateşle ve unut" giriş noktası |
| `AsyncExecute(CancellationToken)` | metot | Beklenebilir giriş noktası |
| `Cancel()` | metot | `CancellationTokenSource` ile iptal eder |

---

### `IProcessFlow<TArgs>`

```csharp
public interface IProcessFlow<TArgs> : IProcessFlow
    where TArgs : IProcessFlowArgs
```

Tiplendirilmiş argüman erişimi için temel arayüze `TArgs Args { get; }` ekler.

---

## Sınıf: `BaseProcessFlow<TArgs>`

```csharp
public abstract class BaseProcessFlow<TArgs> : IProcessFlow<TArgs>
    where TArgs : IProcessFlowArgs
```

### Enjekte Edilen Alanlar

| Tip | Ad | Açıklama |
|---|---|---|
| `IObjectResolver` | `_resolver` | VContainer çözümleyici; alt sınıflarda `Resolve<T>()` için kullanılabilir |

### Temel Özellikler

| Özellik | Tip | Notlar |
|---|---|---|
| `Id` | `string` | `=> GetType().Name` |
| `Args` | `TArgs` | `Initialize(IProcessFlowArgs)` ile ayarlanır |
| `IsCompleted` | `bool` | Devam işlevi tamamlandığında `true` |
| `cancellationTokenSource` | `CancellationTokenSource` | Geç oluşturulur — ilk erişimde yaratılır |

### Soyut Metot

```csharp
public abstract UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx);
```

Tüm asenkron iş burada gerçekleşir. Eklenmiş alt flow'ları çağırın veya diğer görevleri bekleyin. Bu metot döndüğünde devam işlevi eklenmiş flow'ları zincirler ve `onComplete`'i tetikler.

### Execute ve AsyncExecute Karşılaştırması

```csharp
// Ateşle ve unut — asenkron olmayan bağlamdan güvenle çağrılabilir
flow.Execute(cancellationToken);

// Beklenebilir — asenkron bağlamda kullanın
var result = await flow.AsyncExecute(cancellationToken);
```

### Flow Zincirleme

```csharp
flowA
    .AppendProcess(flowB)
    .AppendProcess(flowC)
    .OnComplete(f => Debug.Log("Hepsi tamamlandı"));

flowA.Execute(token);
// Yürütme sırası: flowA → flowB → flowC → onComplete
```

---

## Sınıf: `ProcessFlowProvider`

```csharp
public class ProcessFlowProvider : IDisposable
```

Aktif flow'lar için merkezi kayıt. Uygulama kapsamında `Singleton` olarak kaydedin.

### Temel Metotlar

| Metot | Açıklama |
|---|---|
| `CreateProcessFlow<TArgs, TFlow>(TArgs args)` | `new()` ile `TFlow` oluşturur, VContainer ile enjekte eder, argümanlarla başlatır, kayıt defterine ekler |
| `GetProcessFlow<TFlow>()` | `TFlow` tipinden ilk aktif flow'u döndürür |
| `GetProcessFlows<TFlow>()` | `TFlow` tipinden tüm aktif flow'ları döndürür |
| `RemoveProcessFlow<TFlow>()` | İlk eşleşeni iptal eder, serbest bırakır ve kaldırır |
| `RemoveProcessFlows<TFlow>()` | Tüm eşleşenleri iptal eder, serbest bırakır ve kaldırır |
| `RemoveAllProcessFlows()` | Tümünü iptal eder, serbest bırakır ve temizler |
| `Dispose()` | `RemoveAllProcessFlows()`'u çağırır |

> `AddProcessFlow`, flow tamamlandığında onu kayıt defterinden otomatik kaldıran bir `onComplete` geri çağrısı kaydeder.

---

## DI Kaydı

### Provider'ı kaydetme

```csharp
builder.Register<ProcessFlowProvider>(Lifetime.Singleton).AsSelf();
```

### Flow fabrikası kaydetme

```csharp
// LifetimeScope.Configure içinde:
builder.RegisterProcessFlow<MyFlowArgs, MyFlow>(Lifetime.Singleton);
// Func<MyFlowArgs, MyFlow> fabrikası kaydeder
```

```csharp
// Uzantı metodu uygulaması:
public static void RegisterProcessFlow<TArgs, TProcessFlow>(
    this IContainerBuilder builder, Lifetime lifetime)
    where TArgs : IProcessFlowArgs
    where TProcessFlow : IProcessFlow<TArgs>, new()
```

---

## Somut Flow Örneği

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VGameKit.Runtime.ProcessFlows;
using VGameKit.Runtime.Log;

public class LoadLevelArgs : IProcessFlowArgs
{
    public int LevelIndex;
}

public class LoadLevelFlow : BaseProcessFlow<LoadLevelArgs>
{
    [Inject] private readonly ISceneLoader _sceneLoader;

    public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
    {
        GKLog.Log(LogState.ProcessFlow, $"LoadLevelFlow: {Args.LevelIndex}. seviye yükleniyor");
        await _sceneLoader.LoadAsync(Args.LevelIndex, ctx);
        GKLog.Log(LogState.ProcessFlow, "LoadLevelFlow: tamamlandı");
        return this;
    }
}
```

### Kullanım

```csharp
// ProcessFlowProvider'ı enjekte et
[Inject] private readonly ProcessFlowProvider _flowProvider;

// Flow'u oluştur ve başlat
var flow = _flowProvider.CreateProcessFlow<LoadLevelArgs, LoadLevelFlow>(
    new LoadLevelArgs { LevelIndex = 2 });

flow.OnComplete(_ => ShowLevelUI());
flow.Execute(destroyCancellationToken);
```

---

## Ayrıca Bakınız

- `AbsAppManager.StartAsync` — flow'lar kullanılmadan önce yaygın giriş noktası
- `ProcessFlowProvider` — tüm aktif flow'ları yönetir
- UniTask — asenkron görev kütüphanesi
- VContainer `IObjectResolver` — flow'larda alan enjeksiyonu için kullanılır
