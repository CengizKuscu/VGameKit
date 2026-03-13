# Havuz Sistemi (Spawner) Referans

Bu belge Spawner sistemi için Türkçe referanstır.

## Genel Bakış / Overview
`BaseSpawnPool` ve türevi sınıflar, nesne havuzlama (object pooling) mantığını sağlar.

## Ana Tipler / Key Types
- `ISpawnItem` / `ISpawnItem<TModel>` – Havuzlanabilir öğe arayüzleri
- `BaseSpawnPool` / `BaseSpawnPool<TModel, TItem>` – Havuzun temel ve türetilmiş sınıfları
- `SpawnerExtensions` – DI’ya havuzla ilgili kayıtlar ekleyen uzantılar

## Örnek / Example
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
