namespace VGameKit.Runtime.UI.Menu
{
    /// <summary>
    /// Interface for menu views in the VGameKit runtime.
    /// </summary>
    public interface IMenu
    {
        /// <summary>
        /// Sets the active state of the menu.
        /// </summary>
        /// <param name="active">If set to <c>true</c>, activates the menu; otherwise, deactivates it.</param>
        void SetActive(bool active);
        
        /// <summary>
        /// Gets a value indicating whether the menu is currently active.
        /// </summary>
        bool ActiveSelf { get; }
        
        /// <summary>
        /// Gets a value indicating whether the menu should be destroyed when closed.
        /// </summary>
        bool DestroyWhenClosed { get; }
        
        /// <summary>
        /// Initializes the menu view.
        /// </summary>
        void InitializeView();
        
        /// <summary>
        /// Updates the menu view.
        /// </summary>
        void UpdateView();
        
        /// <summary>
        /// Opens the menu.
        /// </summary>
        void Open();
        
        /// <summary>
        /// Closes the menu.
        /// </summary>
        void Close();
    
        /// <summary>
        /// Disposes the menu and releases resources.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c>, managed resources should be disposed.</param>
        void Dispose(bool disposing);
    }
}