# Explanation: Object Pooling and the Spawner System

## Why object pooling

`Instantiate` and `Destroy` are expensive in Unity. Each call allocates memory, triggers garbage collection, and involves the physics and rendering subsystems. For frequently created objects — enemies, projectiles, particles — this produces frame-rate spikes.

Object pooling solves this by reusing a fixed set of pre-created instances. An object is not destroyed when it is no longer needed; it is deactivated and returned to the pool. When a new instance is needed, the pool reactivates an existing one instead of allocating.

---

## BaseSpawnPool architecture

`BaseSpawnPool<TModel, TItem>` is VGameKit's pooling foundation:

```
BaseSpawnPool<TModel, TItem>
├── _spawnFunc: Func<TModel, Transform, TItem>   creates new instances
├── _poolTarget: Transform                        parent for pooled GameObjects
├── _inactiveItems: Queue<TItem>                 inactive items ready to reuse
└── _activeItems: List<TItem>                    currently live items
```

### The factory pattern

The pool does not call `Instantiate` directly. It delegates to an injected factory:

```csharp
Func<EnemyModel, Transform, EnemyItem> spawnFunc =
    (model, parent) => Object.Instantiate(prefab, parent);
```

This keeps the pool itself testable and free of prefab references. The factory is registered in the LifetimeScope and injected by VContainer as `_spawnFunc` — the pool never uses `Resources.Load` or scene references.

---

## Spawn lifecycle

```
GetItem(model, parent)
    if _inactiveItems is not empty:
        dequeue item from _inactiveItems
        item.ReInitialize(model)       ← re-configure for new use
        item.gameObject.SetActive(true)
        add to _activeItems
    else if willGrow:
        call _spawnFunc(model, parent)  ← allocate only when pool is exhausted
        add to _activeItems
    else:
        throw InvalidOperationException

ReleaseItem(item)
    item.gameObject.SetActive(false)
    reparent to _poolTarget
    move from _activeItems → _inactiveItems  ← ready for next GetItem

HideAllObjects()
    release every item in _activeItems  ← deactivates all, does not destroy

Dispose()
    RemoveAllObjects()                  ← calls HideAllObjects(), then destroys
    clear both collections
```

---

## ISpawnItem contract

Every poolable item implements two methods:

| Method | Called when | Purpose |
|---|---|---|
| `ReInitialize(model)` | Inside `GetItem` | Apply fresh configuration to the item; reset all state |
| `ReleaseItem(this)` | From within the item | Return the item to its pool; triggers deactivation |

`ReInitialize` is called on both new and reused instances, so items must fully reset themselves from the model — never assume state carried over from a previous use. `GetItem` calls `ReInitialize` automatically; callers must not call it again after `GetItem`.

---

## Lifetime and disposal

Pools registered as `Lifetime.Scoped` have `Dispose()` called automatically by VContainer when the scope ends (scene unload, app shutdown). This destroys all remaining GameObjects without requiring manual cleanup.

Pools registered as `Lifetime.Singleton` persist across scene loads. If a singleton pool is used for scene-specific objects, call `Dispose()` manually when the scene ends, or use `Lifetime.Scoped` instead.

Registering as `Lifetime.Singleton` without explicit disposal risks leaking GameObjects across scene loads.

---

## What to avoid

- **Calling `Instantiate` inside game logic**: route all creation through the pool via `GetItem`.
- **Holding references to released items**: after `ReleaseItem`, the item may be given to a different caller at any time.
- **Re-creating objects in `ReInitialize`**: `ReInitialize` should reconfigure, not allocate. If `ReInitialize` calls `Instantiate`, you have a pool that leaks.
- **Calling `ReInitialize` manually after `GetItem`**: `GetItem` already calls it — calling it twice produces double-initialization bugs.
- **Static pools**: static state survives scene loads and produces stale references.

---

## See also

- [Reference: Spawner System](../reference/api/spawner.md)
- [How-to: Use the spawner](../how-to/use-spawner.md)
- [Tutorial: Spawner basics](../tutorials/spawner-basics.md)
- [Explanation: VContainer and DI](./vcontainer-di.md)
