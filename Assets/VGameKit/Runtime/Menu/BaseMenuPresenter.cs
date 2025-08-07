using System;
using VContainer;
using VGameKit.Runtime.Core;

namespace VGameKit.Runtime.Menu
{
    /// <summary>
    /// Abstract base class for menu presenters.
    /// Handles menu view creation, opening, closing, and destruction.
    /// </summary>
    /// <typeparam name="TMenuName">Enum type representing menu names.</typeparam>
    /// <typeparam name="TData">Type of menu data.</typeparam>
    /// <typeparam name="TMenu">Type of menu view.</typeparam>
    public abstract class BaseMenuPresenter<TMenuName, TData, TMenu> : SubscribableConcrete, IMenuPresenter<TMenuName>
        where TMenuName : Enum
        where TData : MenuData
        where TMenu : BaseMenuView, IMenu
    {
        /// <summary>
        /// Factory for creating menu views.
        /// </summary>
        [Inject] protected readonly Func<TMenu> _menuFactory;

        /// <summary>
        /// The current menu view instance.
        /// </summary>
        protected TMenu _view;

        /// <summary>
        /// The data associated with the menu.
        /// </summary>
        protected TData MenuData { get; private set; }

        /// <summary>
        /// Gets the current menu view.
        /// </summary>
        public TMenu View => _view;

        /// <summary>
        /// Indicates whether the menu is already opened.
        /// </summary>
        public bool IsAlreadyOpened
        {
            get { return (_view != null && _view.ActiveSelf); }
        }

        /// <summary>
        /// Gets the menu mode.
        /// </summary>
        public abstract MenuMode MenuMode { get; }

        /// <summary>
        /// Gets the menu name.
        /// </summary>
        public abstract TMenuName MenuName { get; }

        /// <summary>
        /// Sets the menu view instance.
        /// </summary>
        /// <param name="view">The menu view to set.</param>
        private void SetView(TMenu view)
        {
            _view = view;
        }

        /// <summary>
        /// Gets the menu view as a specific type.
        /// </summary>
        /// <typeparam name="TMenu1">The type to cast the view to.</typeparam>
        /// <returns>The view as <typeparamref name="TMenu1"/>.</returns>
        public TMenu1 GetView<TMenu1>() where TMenu1 : BaseMenuView, IMenu
        {
            return View as TMenu1;
        }

        /// <summary>
        /// Gets or creates the menu view and initializes it with the provided data.
        /// </summary>
        /// <param name="menuData">The data to initialize the menu with.</param>
        /// <returns>The menu view instance.</returns>
        protected TMenu GetMenu(TData menuData = null)
        {
            if (_view == null)
            {
                var view = _menuFactory();
                view.InitializeView();
                SetView(view);
                view.SetActive(false);
                MenuData = menuData;
                OnShow(menuData);
            }
            else
            {
                _view.UpdateView();
                MenuData = menuData;
                UpdatePresenter();
                OnShow(menuData);
            }

            return _view;
        }

        /// <summary>
        /// Opens the menu with the specified data.
        /// </summary>
        /// <typeparam name="TData1">Type of the menu data.</typeparam>
        /// <param name="data">The data to open the menu with.</param>
        public void Open<TData1>(TData1 data) where TData1 : MenuData
        {
            Open(data as TData);
        }

        /// <summary>
        /// Opens the menu with the specified data.
        /// </summary>
        /// <param name="data">The data to open the menu with.</param>
        public void Open(TData data = null)
        {
            GetMenu(data);
            _view?.Open();
        }

        /// <summary>
        /// Closes the menu and destroys the view if necessary.
        /// </summary>
        public void Close()
        {
            OnHide();

            _view?.Close();
            if (_view != null && _view.DestroyWhenClosed)
            {
                DestroyView();
            }
        }

        /// <summary>
        /// Destroys the current menu view and disposes its resources.
        /// </summary>
        public void DestroyView()
        {
            if (_view != null)
            {
                OnHide();
                _view.Dispose(true);
                _view = null;
            }
        }

        /// <summary>
        /// Called when the menu is shown.
        /// </summary>
        /// <param name="menuData">The data associated with the menu.</param>
        protected virtual void OnShow(TData menuData)
        {
        }

        /// <summary>
        /// Called when the menu is hidden.
        /// </summary>
        protected virtual void OnHide()
        {
        }

        /// <summary>
        /// Called to update the presenter.
        /// </summary>
        protected virtual void UpdatePresenter()
        {
        }
    }
}