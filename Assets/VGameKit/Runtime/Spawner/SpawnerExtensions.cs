using System.Runtime.CompilerServices;
using VContainer;
using VContainer.Unity;

namespace VGameKit.Runtime.Spawner
{
    /// <summary>
    /// Provides extension methods for registering object spawners in the dependency injection container.
    /// </summary>
    public static class SpawnerExtensions
    {
        /// <summary>
        /// Registers a factory for spawning objects of type <typeparamref name="TItem"/> with the specified model and transform.
        /// </summary>
        /// <typeparam name="TModel">The model type implementing <see cref="ISpawnItemModel"/>.</typeparam>
        /// <typeparam name="TTransform">The transform type, must inherit from <see cref="UnityEngine.Transform"/>.</typeparam>
        /// <typeparam name="TItem">The item type, must inherit from <see cref="UnityEngine.Component"/> and implement <see cref="ISpawnItem{TModel}"/>.</typeparam>
        /// <param name="builder">The container builder to register the factory with.</param>
        /// <param name="item">The prefab or item to be spawned.</param>
        /// <param name="lifetime">The lifetime of the registered factory.</param>
        /// <param name="isInjectable">If true, uses dependency injection to instantiate the item; otherwise, uses Unity's Instantiate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterObjectSpawner<TModel, TTransform, TItem>(this IContainerBuilder builder, TItem item,
            Lifetime lifetime, bool isInjectable = true)
            where TTransform : UnityEngine.Transform
            where TModel : ISpawnItemModel
            where TItem : UnityEngine.Component, ISpawnItem<TModel>
        {
            builder.RegisterFactory<TModel, TTransform, TItem>(container =>
            {
                return (model, parent) =>
                {
                    var cloneItem = isInjectable
                        ? container.Instantiate(item, parent)
                        : UnityEngine.Object.Instantiate(item, parent);
    
                    if (model != null)
                        cloneItem.ReInitialize(model);
                    else
                    {
                        cloneItem.gameObject.SetActive(false);
                    }
    
                    return cloneItem;
                };
            }, lifetime);
        }
    }
}