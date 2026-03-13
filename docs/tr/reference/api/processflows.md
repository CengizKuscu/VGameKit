# ProcessFlows Referans

Bu belge, ProcessFlows altyapısının Türkçe referansını içerir.

## Genel Bakış / Overview
ProcessFlow, oyun akışlarını yönetmek için kullanılan akış işleyiş mekanizmasıdır. Bu bölümde temel tipler ve kullanım örnekleri sunulur.

## Ana Tipler / Key Types
- `IProcessFlow` ve `IFlowTask` arayüzleri
- `ProcessFlowProvider` ve `BaseProcessFlow` gibi temel sınıflar

## Örnek / Example
```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using VGameKit.Runtime.ProcessFlows;

public class DemoFlow : IFlowAsyncTask
{
    public UniTask ExecuteAsync(CancellationToken token)
    {
        return UniTask.CompletedTask;
    }
}
```

## Bkz./See Also
- `ProcessFlowProvider`
- `IFlowTask`
