using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VGameKit.Runtime.Core;

namespace VGameKit.Runtime.Menu
{
    /// <summary>
    /// Abstract base class for managing UI menus.
    /// Handles opening, closing, and tracking menu presenters of type <typeparamref name="TMenuName"/>.
    /// </summary>
    /// <typeparam name="TMenuName">Enum type representing menu names.</typeparam>
    public abstract class BaseMenuManager<TMenuName> : SubscribableMonoBehaviour, IMenuManager<TMenuName>
        where TMenuName : Enum
    {
        /// <summary>
        /// The root transform under which menu objects are parented.
        /// </summary>
        [SerializeField] protected Transform _menuRoot;

        /// <summary>
        /// List of currently managed menu presenters.
        /// </summary>
        private readonly List<IMenuPresenter<TMenuName>> _menuList = new(); // Initialize inline

        /// <summary>
        /// Gets the menu root transform.
        /// </summary>
        public Transform MenuRoot => _menuRoot;

        /// <summary>
        /// Initializes the menu manager by clearing the menu list and destroying all child menu objects.
        /// </summary>
        public override void Init()
        {
            _menuList.Clear();

            if (_menuRoot is not null)
            {
                foreach (Transform o in _menuRoot)
                {
                    Destroy(o.gameObject);
                }
            }
        }

        /// <summary>
        /// Opens a menu using the specified presenter and menu data.
        /// Handles single menu mode and prevents duplicate presenters.
        /// </summary>
        /// <typeparam name="TPresenter">Type of the menu presenter.</typeparam>
        /// <typeparam name="TMenuModel">Type of the menu data model.</typeparam>
        /// <param name="menuPresenter">The menu presenter instance.</param>
        /// <param name="menuData">Optional menu data.</param>
        protected void Open<TPresenter, TMenuModel>(TPresenter menuPresenter, MenuData menuData = null)
            where TMenuModel : MenuData
            where TPresenter : IMenuPresenter<TMenuName>
        {
            if (menuPresenter == null) return; // Handle null presenter

            if (menuPresenter.MenuMode == MenuMode.Single)
            {
                CloseOthers();
            }

            var existingPresenter = _menuList.Find(s => Equals(s.MenuName, menuPresenter.MenuName));

            if (existingPresenter == null)
            {
                _menuList.Add(menuPresenter);
                menuPresenter.Open(menuData as TMenuModel);
            }
            else
            {
                existingPresenter.Open(menuData as TMenuModel); // Consider handling existing data explicitly
            }
        }

        /// <summary>
        /// Abstract method to open a menu by name and data.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <param name="menuName">The menu name.</param>
        /// <param name="menuData">The menu data.</param>
        protected abstract void OpenMenu(TMenuName menuName, MenuData menuData);

        /// <summary>
        /// Closes the menu with the specified name if it is open.
        /// </summary>
        /// <param name="menuName">The menu name to close.</param>
        protected void CloseMenu(TMenuName menuName)
        {
            var presenter = _menuList.Find(s => s.IsAlreadyOpened && Equals(s.MenuName, menuName));
            if (presenter != null)
            {
                presenter.Close();
                _menuList.Remove(presenter); // Remove after closing
            }
        }

        /// <summary>
        /// Closes all open menus and removes them from the list.
        /// </summary>
        protected void CloseOthers()
        {
            for (int i = _menuList.Count - 1; i >= 0; i--) // Iterate backwards for safe removal
            {
                var presenter = _menuList[i];
                if (presenter.IsAlreadyOpened)
                {
                    presenter.Close();
                }

                _menuList.RemoveAt(
                    i); // Remove all, regardless of open state, since CloseOthers() implies closing *all*
            }
        }

        /// <summary>
        /// Closes all open menus except those specified in the parameters.
        /// </summary>
        /// <param name="menuNames">Menu names to keep open.</param>
        protected void CloseOthers(params TMenuName[] menuNames)
        {
            for (int i = _menuList.Count - 1; i >= 0; i--) // Iterate backwards for safe removal
            {
                var presenter = _menuList[i];
                if (presenter.IsAlreadyOpened && !menuNames.Contains(presenter.MenuName))
                {
                    presenter.Close();
                    _menuList.RemoveAt(i); // Remove after closing
                }
            }
        }
    }
}