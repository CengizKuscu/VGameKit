namespace VGameKit.Runtime.UI.Menu.Events
{
    /// <summary>
    /// Represents a base event for closing a menu, parameterized by the menu name enum.
    /// </summary>
    /// <typeparam name="TMenuName">The enum type representing the menu name.</typeparam>
    public class BaseCloseMenuEvent<TMenuName> where TMenuName : System.Enum
    {
        /// <summary>
        /// Gets the name of the menu to be closed.
        /// </summary>
        public TMenuName MenuName { get; }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCloseMenuEvent{TMenuName}"/> class.
        /// </summary>
        /// <param name="menuName">The name of the menu to close.</param>
        public BaseCloseMenuEvent(TMenuName menuName)
        {
            MenuName = menuName;
        }
    }
}