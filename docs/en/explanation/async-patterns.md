# Explanation: Async Patterns with UniTask

## Why UniTask instead of Task or coroutines

Unity's `MonoBehaviour` coroutines are convenient but have significant limitations: they cannot return values, error handling is awkward, and they do not compose. The standard `System.Threading.Tasks.Task` works, but it allocates on the heap for each continuation and integrates poorly with Unity's single-threaded game loop.

**UniTask** (Cysharp) is allocation-free, integrates with Unity's `PlayerLoop`, supports cancellation via `CancellationToken`, and composes with the same `async/await` syntax as `Task`. VGameKit uses UniTask throughout.

---

## UniTask vs UniTaskVoid

| Type | Use when | Returns |
|---|---|---|
| `async UniTask` | Caller may `await` the result | Awaitable, can propagate exceptions |
| `async UniTaskVoid` | Fire-and-forget; no caller awaits | Not awaitable; exceptions log but do not propagate |

Use `UniTask` for process flows and any async method where the caller needs to sequence on completion. Use `UniTaskVoid` for event handlers and Unity lifecycle callbacks (`Start`, button callbacks) that must be async but are never awaited.

```csharp
// Correct: fire-and-forget in a MonoBehaviour
private async UniTaskVoid OnButtonClickedAsync()
{
    await LoadMenuAsync(destroyCancellationToken);
}

// Correct: awaitable flow step
public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
{
    await DoWorkAsync(ctx);
    return this;
}
```

---

## CancellationToken propagation

Every async method in VGameKit accepts a `CancellationToken`. Pass it through every `await` call:

```csharp
public async UniTask LoadAsync(CancellationToken ctx)
{
    await DownloadAsync(ctx);         // propagates cancellation
    await ParseAsync(ctx);            // propagates cancellation
}
```

**Do not** create `CancellationTokenSource` inside a method unless you need a timeout. Prefer the token passed in from above. The built-in `MonoBehaviour.destroyCancellationToken` cancels when the GameObject is destroyed.

`ProcessFlowProvider` creates one `CancellationTokenSource` per flow. Calling `RemoveProcessFlow<T>()` cancels that source, which propagates through the entire `await` chain inside the flow.

---

## Awaiting Unity events

UniTask provides awaitable wrappers for common Unity patterns:

```csharp
// Wait one frame
await UniTask.Yield();

// Wait N milliseconds
await UniTask.Delay(500, cancellationToken: ctx);

// Wait until a condition is true
await UniTask.WaitUntil(() => _isReady, cancellationToken: ctx);

// Wait for the next FixedUpdate
await UniTask.WaitForFixedUpdate();
```

Always pass `cancellationToken: ctx` to these calls. Without it, the operation continues after scope disposal and may access destroyed objects.

---

## Error handling in async flows

UniTask exceptions propagate like standard `await` exceptions. Wrap external SDK calls:

```csharp
public override async UniTask<IProcessFlow> AsyncExecute(CancellationToken ctx)
{
    try
    {
        await _sdk.InitAsync(ctx);
    }
    catch (OperationCanceledException)
    {
        // expected on cancel; do not log as error
        throw;
    }
    catch (System.Exception ex)
    {
        GKLog.Log(LogState.Error, $"SDK init failed: {ex.Message}");
        // degrade gracefully; do not rethrow unless unrecoverable
    }
    return this;
}
```

Re-throw `OperationCanceledException` so VGameKit's cancellation machinery can clean up correctly.

---

## What to avoid

- **Blocking on UniTask**: `task.GetAwaiter().GetResult()` on the main thread deadlocks. Always `await`.
- **Forgetting cancellation tokens**: without tokens, async operations outlive their scopes and reference destroyed objects.
- **`async void`**: Unity may silently swallow exceptions. Use `async UniTaskVoid` instead.
- **Nested `CancellationTokenSource` without `using`**: sources must be disposed. Prefer `using var cts = new CancellationTokenSource()`.

---

## See also

- [Reference: ProcessFlows](../reference/api/processflows.md)
- [Reference: App Manager](../reference/api/app-manager.md)
- [Tutorial: Learning Process Flows](../tutorials/processflows.md)
