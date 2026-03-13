## Subscribable System Reference

English reference for the subscription system in VGameKit.

## Overview
Abonelik management is handled via `ISubscribableObject`, `SubscribableMonoBehaviour`, and `SubscribableConcrete` with Disposable patterns to prevent memory leaks.

## Core Types
- `ISubscribableObject` – Basic subscribable interface
- `SubscribableMonoBehaviour` – MonoBehaviour-based subscription management
- `SubscribableConcrete` – Non-MonoBehaviour subscription manager base class

## Example
```csharp
public class MyComponent : SubscribableMonoBehaviour
{
    protected override void Init()
    {
        // initialization
    }
}
```

## See Also
- `AppReadyEvent` (example events)
- `MessagePipe` documentation
