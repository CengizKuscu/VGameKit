using UnityEngine;

namespace VGameKit.Runtime.UI.Menu
{
    /// <summary>
    /// Base class for menu views in the game. Handles activation, initialization, and disposal logic.
    /// </summary>
    public class BaseMenuView : MonoBehaviour, IMenu
    {
        /// <summary>
        /// If true, the menu GameObject will be destroyed when closed.
        /// </summary>
        [SerializeField] private bool _destroyWhenClosed;
        
        /// <summary>
        /// Returns true if the menu GameObject is active in the hierarchy.
        /// </summary>
        public bool ActiveSelf => gameObject.activeInHierarchy;
        
        /// <summary>
        /// Returns whether the menu should be destroyed when closed.
        /// </summary>
        public bool DestroyWhenClosed => _destroyWhenClosed;
        
        /// <summary>
        /// Sets the active state of the menu GameObject.
        /// </summary>
        /// <param name="active">Whether to activate or deactivate the menu.</param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    
        /// <summary>
        /// Initializes the menu view's transform to default values.
        /// </summary>
        public void InitializeView()
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
        }
    
        /// <summary>
        /// Updates the menu view by reinitializing its transform.
        /// </summary>
        public void UpdateView()
        {
            InitializeView();
        }
    
        /// <summary>
        /// Opens the menu by setting it active.
        /// </summary>
        public void Open()
        {
            SetActive(true);
        }
    
        /// <summary>
        /// Closes the menu by setting it inactive.
        /// </summary>
        public void Close()
        {
            SetActive(false);
        }
        
        /// <summary>
        /// Disposes the menu by deactivating it.
        /// </summary>
        public void Dispose()
        {
            SetActive(false);
        }
    
        /// <summary>
        /// Disposes the menu and optionally destroys the GameObject if required.
        /// </summary>
        /// <param name="disposing">If true and <see cref="_destroyWhenClosed"/> is true, destroys the GameObject.</param>
        public void Dispose(bool disposing)
        {
            Dispose();
            if (disposing && _destroyWhenClosed)
                Destroy(gameObject);
        }
    }
}