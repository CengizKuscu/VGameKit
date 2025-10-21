namespace VGameKit.Runtime.UI.Menu
{
    /// <summary>
    /// Represents a presenter for a menu, providing methods to open, close, and manage menu views.
    /// </summary>
    /// <typeparam name="TMenuName">The enum type representing the menu name.</typeparam>
    public interface IMenuPresenter<TMenuName> : System.IDisposable where TMenuName : System.Enum
    {
        /// <summary>
        /// Gets the name of the menu.
        /// </summary>
        TMenuName MenuName { get; }
    
        /// <summary>
        /// Gets the mode of the menu.
        /// </summary>
        MenuMode MenuMode { get; }
    
        /// <summary>
        /// Closes the menu.
        /// </summary>
        void Close();
    
        /// <summary>
        /// Destroys the menu view.
        /// </summary>
        void DestroyView();
    
        /// <summary>
        /// Gets a value indicating whether the menu is already opened.
        /// </summary>
        bool IsAlreadyOpened { get; }
    
        /// <summary>
        /// Opens the menu with the specified data.
        /// </summary>
        /// <typeparam name="TData">The type of data used to open the menu.</typeparam>
        /// <param name="data">The data to open the menu with.</param>
        void Open<TData>(TData data) where TData : MenuData;
    
        /// <summary>
        /// Gets the menu view of the specified type.
        /// </summary>
        /// <typeparam name="TMenu">The type of the menu view.</typeparam>
        /// <returns>The menu view instance.</returns>
        TMenu GetView<TMenu>() where TMenu : BaseMenuView, IMenu;
    }
}