using System;
using System.Collections.Generic;
using UnityEngine;

namespace VGameKit.Runtime.Spawner
{
    /// <summary>
    /// Base class for spawn pools, providing basic pool configuration and disposal logic.
    /// </summary>
    public class BaseSpawnPool : IDisposable
    {
        /// <summary>
        /// The initial size of the pool.
        /// </summary>
        protected int _poolSize;

        /// <summary>
        /// Indicates if the pool is allowed to grow beyond its initial size.
        /// </summary>
        protected bool _willGrow;

        /// <summary>
        /// The transform under which pooled objects are parented.
        /// </summary>
        protected Transform _poolTarget;

        /// <summary>
        /// Tracks whether the pool has already been disposed.
        /// </summary>
        protected bool _alreadyDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSpawnPool"/> class.
        /// </summary>
        /// <param name="poolSize">Initial pool size.</param>
        /// <param name="poolTarget">Transform to parent pooled objects under.</param>
        /// <param name="willGrow">Whether the pool can grow.</param>
        public BaseSpawnPool(int poolSize, Transform poolTarget, bool willGrow)
        {
            _alreadyDisposed = false;
            _poolTarget = poolTarget;
            _poolSize = poolSize;
            _willGrow = willGrow;
        }

        /// <summary>
        /// Releases an item back to the pool.
        /// </summary>
        /// <param name="spawnItem">The item to release.</param>
        public virtual void ReleaseItem(ISpawnItem spawnItem)
        {
        }

        /// <summary>
        /// Disposes the pool and its resources.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Disposes the pool with an explicit call.
        /// </summary>
        /// <param name="disposing">True if called explicitly, false if called by the finalizer.</param>
        public virtual void Dispose(bool disposing)
        {
        }
    }

    /// <summary>
    /// Generic base class for spawn pools, managing pooled items of a specific type.
    /// </summary>
    /// <typeparam name="TModel">The model type for spawn items.</typeparam>
    /// <typeparam name="TItem">The MonoBehaviour type implementing ISpawnItem.</typeparam>
    public class BaseSpawnPool<TModel, TItem> : BaseSpawnPool
        where TModel : class, ISpawnItemModel
        where TItem : MonoBehaviour, ISpawnItem<TModel>
    {
        /// <summary>
        /// Function to spawn a new item given a model and parent transform.
        /// </summary>
        private readonly Func<TModel, Transform, TItem> _spawnFunc;

        /// <summary>
        /// Queue of inactive (available) items.
        /// </summary>
        private readonly Queue<TItem> _inactiveItems;

        /// <summary>
        /// List of active (in-use) items.
        /// </summary>
        private readonly List<TItem> _activeItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSpawnPool{TModel, TItem}"/> class.
        /// </summary>
        /// <param name="spawnFunc">Function to create new items.</param>
        /// <param name="poolSize">Initial pool size.</param>
        /// <param name="poolTarget">Transform to parent pooled objects under.</param>
        /// <param name="willGrow">Whether the pool can grow.</param>
        public BaseSpawnPool(Func<TModel, Transform, TItem> spawnFunc, int poolSize, Transform poolTarget,
            bool willGrow)
            : base(poolSize, poolTarget, willGrow)
        {
            _spawnFunc = spawnFunc ?? throw new ArgumentNullException(nameof(spawnFunc));
            _inactiveItems = new Queue<TItem>(poolSize);
            _activeItems = new List<TItem>(poolSize);

            for (int i = 0; i < poolSize; i++)
            {
                TItem item = _spawnFunc(default, poolTarget); // default passes null for reference types
                item.gameObject.SetActive(false);
                item.SpawnPool = this;
                if (_poolTarget != null)
                {
                    item.transform.SetParent(_poolTarget);
                }

                _inactiveItems.Enqueue(item);
            }
        }

        /// <summary>
        /// Retrieves an item from the pool, initializing it with the provided data and parent.
        /// </summary>
        /// <param name="data">The model data for the item.</param>
        /// <param name="parent">Optional parent transform for the item.</param>
        /// <returns>The initialized item.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the pool is empty and cannot grow.</exception>
        public TItem GetItem(TModel data, Transform parent = null)
        {
            TItem item;
            if (_inactiveItems.Count > 0)
            {
                item = _inactiveItems.Dequeue();

                if (parent != null)
                {
                    item.transform.SetParent(parent); // Allow custom parent, fallback to pool    
                }
            }
            else if (_willGrow)
            {
                item = _spawnFunc(data, parent != null ? parent : _poolTarget);
                if (parent != null)
                {
                    item.transform.SetParent(parent);
                }

                item.SpawnPool = this;
            }
            else
            {
                throw new InvalidOperationException("No available items in the pool and pool growth is disabled.");
            }

            item.ReInitialize(data);
            item.gameObject.SetActive(true);
            _activeItems.Add(item);
            return item;
        }

        /// <summary>
        /// Hides all active objects and returns them to the pool.
        /// </summary>
        public void HideAllObjects()
        {
            foreach (var item in _activeItems)
            {
                item.gameObject.SetActive(false);
                if (_poolTarget != null)
                {
                    item.transform.SetParent(_poolTarget);
                }

                _inactiveItems.Enqueue(item);
            }

            _activeItems.Clear();
        }

        /// <summary>
        /// Removes and destroys all objects managed by the pool.
        /// </summary>
        private void RemoveAllObjects()
        {
            foreach (var item in _activeItems)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }

            foreach (var item in _inactiveItems)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }

            _activeItems.Clear();
            _inactiveItems.Clear();
        }

        /// <summary>
        /// Releases an item back to the pool, deactivating and reparenting it.
        /// </summary>
        /// <param name="spawnItem">The item to release.</param>
        /// <exception cref="InvalidCastException">Thrown if the item is not of the expected type.</exception>
        public override void ReleaseItem(ISpawnItem spawnItem)
        {
            if (spawnItem is not TItem item)
                throw new InvalidCastException("Spawn item is not of the expected type.");

            if (_inactiveItems.Contains(item))
                return; // already in pool, double release checked

            item.gameObject.SetActive(false);
            if (_poolTarget != null)
            {
                item.transform.SetParent(_poolTarget);
            }

            _inactiveItems.Enqueue(item);
            _activeItems.Remove(item);
        }

        /// <summary>
        /// Disposes the pool and its resources.
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the pool with an explicit call.
        /// </summary>
        /// <param name="explicitCall">True if called explicitly, false if called by the finalizer.</param>
        public override void Dispose(bool explicitCall)
        {
            if (explicitCall && !_alreadyDisposed)
            {
                RemoveAllObjects();
                _alreadyDisposed = true;
            }
        }

        /// <summary>
        /// Finalizer to ensure resources are released.
        /// </summary>
        ~BaseSpawnPool()
        {
            Dispose(false);
        }
    }
}