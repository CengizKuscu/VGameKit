using System;

namespace VGameKit.Runtime.Spawner
{
    /// <summary>
    /// Represents an item that can be spawned and managed by a spawn pool.
    /// </summary>
    public interface ISpawnItem : IDisposable
    {
        /// <summary>
        /// Gets or sets the spawn pool that manages this item.
        /// </summary>
        BaseSpawnPool SpawnPool { get; set; }

        //void SetActive(bool isActive);

        /// <summary>
        /// Releases the specified spawn item back to the pool or performs cleanup.
        /// </summary>
        /// <param name="item">The item to release.</param>
        void ReleaseItem(ISpawnItem item);
    }

    /// <summary>
    /// Represents a spawnable item with a specific model.
    /// </summary>
    /// <typeparam name="TModel">The type of the item model.</typeparam>
    public interface ISpawnItem<TModel> : ISpawnItem where TModel : ISpawnItemModel
    {
        /// <summary>
        /// Gets or sets the model associated with this spawn item.
        /// </summary>
        TModel ItemModel { get; set; }

        /// <summary>
        /// Reinitializes the spawn item with a new model.
        /// </summary>
        /// <param name="itemModel">The new model to use for reinitialization.</param>
        void ReInitialize(TModel itemModel);
    }
}