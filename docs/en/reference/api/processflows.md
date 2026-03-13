## ProcessFlows Reference

English reference for the ProcessFlows subsystem.

## Overview
ProcessFlow is the workflow mechanism used to manage game states and sequences. This section covers core interfaces and usage.

## Key Types
- `IProcessFlow`, `IFlowTask`
- `ProcessFlowProvider`, `BaseProcessFlow` etc.

## Example
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

## See Also
- `ProcessFlowProvider`
- `IFlowTask`
