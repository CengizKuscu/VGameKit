namespace VGameKit.Runtime.UI.Menu
{
    /// <summary>
    /// Specifies the mode in which a menu is loaded.
    /// </summary>
    public enum MenuMode
    {
        /// <summary>
        /// Loads the menu additively, keeping existing menus active.
        /// </summary>
        Additive,
        /// <summary>
        /// Loads the menu exclusively, closing other menus.
        /// </summary>
        Single
    }
}