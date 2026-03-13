## Spawner System Reference

English reference for the Spawner system.

## Overview
BaseSpawnPool and derived classes provide object pooling for efficient spawning.

## Key Types
- `ISpawnItem` / `ISpawnItem<TModel>` – spawn item interfaces
- `BaseSpawnPool` / `BaseSpawnPool<TModel, TItem>` – pool base and generics
- `SpawnerExtensions` – DI extension methods for registering spawners

## Example
```csharp
public class MyModel : ISpawnItemModel { }
public class MyItem : MonoBehaviour, ISpawnItem<MyModel>
{
    public BaseSpawnPool SpawnPool { get; set; }
    public void ReleaseItem(ISpawnItem item) { SpawnPool?.ReleaseItem(item); }
    public void ReInitialize(MyModel m) { /* ... */ }
}
```

## See Also
- `BaseSpawnPool` / `BaseSpawnPool<TModel, TItem>`
- `ISpawnItem` / `ISpawnItemModel`
- `SpawnerExtensions`
