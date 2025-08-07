using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using VContainer;

namespace VGameKit.Runtime.Menu
{
    /// <summary>
    /// Provides extension methods for registering menu factories in the dependency injection container.
    /// </summary>
    public static class MenuExtensions
    {
        /// <summary>
        /// Registers a factory for creating menu views of type <typeparamref name="TMenu"/> in the DI container.
        /// The factory resolves the presenter, attempts to get an existing view, and instantiates a new one if necessary.
        /// </summary>
        /// <typeparam name="TMenuName">The enum type representing menu names.</typeparam>
        /// <typeparam name="TPresenter">The presenter type implementing <see cref="IMenuPresenter{TMenuName}"/>.</typeparam>
        /// <typeparam name="TMenu">The menu view type, must inherit from <see cref="BaseMenuView"/> and implement <see cref="IMenu"/>.</typeparam>
        /// <param name="builder">The DI container builder.</param>
        /// <param name="prefab">The prefab to instantiate if no existing view is found.</param>
        /// <param name="parent">The parent transform for the instantiated menu view.</param>
        /// <param name="lifetime">The lifetime scope for the registered factory.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterMenuFactory<TMenuName, TPresenter, TMenu>(this IContainerBuilder builder,
            TMenu prefab, Transform parent, Lifetime lifetime)
            where TMenuName : Enum
            where TMenu : BaseMenuView, IMenu
            where TPresenter : IMenuPresenter<TMenuName>
        {
            builder.RegisterFactory<TMenu>(resolver =>
            {
                return () =>
                {
                    var presenter = resolver.Resolve<TPresenter>();
                    var menuView = presenter.GetView<TMenu>();
                    if (menuView is null)
                    {
                        menuView = UnityEngine.Object.Instantiate<TMenu>(prefab, parent);
                    }
    
                    return menuView;
                };
            }, lifetime);
        }
    }
}