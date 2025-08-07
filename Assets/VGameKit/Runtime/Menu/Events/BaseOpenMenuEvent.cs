namespace VGameKit.Runtime.Menu.Events
{
    /// <summary>
    /// Represents a base event for opening a menu, containing the menu name and associated data.
    /// </summary>
    /// <typeparam name="TMenuName">The enum type representing the menu name.</typeparam>
    /// <typeparam name="TMenuData">The type of data associated with the menu, derived from <see cref="MenuData"/>.</typeparam>
    public class BaseOpenMenuEvent<TMenuName, TMenuData> where TMenuName : System.Enum where TMenuData : MenuData
    {
        /// <summary>
        /// Gets the name of the menu to open.
        /// </summary>
        public TMenuName MenuName { get; }
    
        /// <summary>
        /// Gets the data associated with the menu.
        /// </summary>
        public TMenuData MenuData { get; }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseOpenMenuEvent{TMenuName, TMenuData}"/> class.
        /// </summary>
        /// <param name="menuName">The name of the menu to open.</param>
        /// <param name="menuData">The data associated with the menu.</param>
        public BaseOpenMenuEvent(TMenuName menuName, TMenuData menuData)
        {
            MenuName = menuName;
            MenuData = menuData;
        }
    }
}