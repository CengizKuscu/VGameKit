namespace VGameKit.Runtime.UI.Menu.Events
{
    /// <summary>
    /// Represents an event to close all menus except those specified by <paramref name="keepMenuNames"/>.
    /// </summary>
    /// <typeparam name="TMenuName">The enum type representing menu names.</typeparam>
    public class BaseCloseOthersMenuEvent<TMenuName> where TMenuName : System.Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCloseOthersMenuEvent{TMenuName}"/> class.
        /// </summary>
        /// <param name="keepMenuNames">The menu names to keep open.</param>
        public BaseCloseOthersMenuEvent(params TMenuName[] keepMenuNames)
        {
        }
    }
}