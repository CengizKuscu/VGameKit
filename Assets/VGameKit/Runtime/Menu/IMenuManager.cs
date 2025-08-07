using UnityEngine;

namespace VGameKit.Runtime.Menu
{
    /// <summary>
    /// Interface for managing menus in the UI system.
    /// </summary>
    /// <typeparam name="TMenuName">
    /// Enum type representing menu names.
    /// </typeparam>
    public interface IMenuManager<TMenuName> where TMenuName : System.Enum
    {
        /// <summary>
        /// Gets the root transform under which all menus are organized.
        /// </summary>
        Transform MenuRoot { get; }
    }
}