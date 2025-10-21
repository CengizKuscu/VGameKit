using System;
using UnityEngine;

namespace VGameKit.Runtime.UI.Popup
{
    /// <summary>
    /// Base class for popups in the UI system.
    /// Handles popup lifecycle, initialization, and builder/model assignment.
    /// </summary>
    /// <typeparam name="TPopupName">Enum type representing popup names.</typeparam>
    public class BasePopup<TPopupName> : MonoBehaviour, IDisposable where TPopupName : Enum
    {
        /// <summary>
        /// The name of the popup, serialized for Unity inspector.
        /// </summary>
        [SerializeField] private TPopupName _popupName;
    
        /// <summary>
        /// Reference to the popup builder.
        /// </summary>
        protected BasePopupBuilder<TPopupName> _builder;
    
        /// <summary>
        /// Indicates whether the popup has been initialized.
        /// </summary>
        protected bool _isInitialized = false;
    
        /// <summary>
        /// Reference to the popup model.
        /// </summary>
        protected BasePopupModel<TPopupName> _model;
    
        /// <summary>
        /// Sets the active state of the popup GameObject.
        /// </summary>
        /// <param name="isActive">Whether the popup should be active.</param>
        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    
        /// <summary>
        /// Gets the name of the popup.
        /// </summary>
        public TPopupName Name => _popupName;
    
        /// <summary>
        /// Disposes the popup by destroying its GameObject.
        /// </summary>
        public void Dispose()
        {
            Destroy(gameObject);
        }
    
        /// <summary>
        /// Assigns a builder to the popup.
        /// </summary>
        /// <param name="builder">The popup builder.</param>
        public void SetBuilder(BasePopupBuilder<TPopupName> builder)
        {
            _builder = builder;
        }
    
        /// <summary>
        /// Initializes the popup with a model and sets it inactive.
        /// </summary>
        /// <param name="model">The popup model.</param>
        public void Init(BasePopupModel<TPopupName> model)
        {
            SetModel(model);
            _isInitialized = true;
            SetActive(false);
        }
    
        /// <summary>
        /// Sets the popup model.
        /// </summary>
        /// <param name="model">The popup model.</param>
        private void SetModel(BasePopupModel<TPopupName> model)
        {
            _model = model;
        }
    
        /// <summary>
        /// Opens the popup, triggering show events and setting it active.
        /// </summary>
        public virtual void Open()
        {
            OnShowBefore();
            SetActive(true);
            OnShowAfter();
        }
        
        /// <summary>
        /// Closes the popup, triggering close events and hiding it.
        /// </summary>
        public virtual void Close()
        {
            _model?.OnClose?.Invoke(_popupName);
            OnHidePopup();
            _builder.ClosePopup(this);
        }
    
        /// <summary>
        /// Called when the popup is hidden. Can be overridden.
        /// </summary>
        public virtual void OnHidePopup(){}
    
        /// <summary>
        /// Called before the popup is shown. Can be overridden.
        /// </summary>
        public virtual void OnShowBefore(){}
        
        /// <summary>
        /// Called after the popup is shown. Can be overridden.
        /// </summary>
        public virtual void OnShowAfter(){}
        
        /// <summary>
        /// Breaks the popup flow, disposing and clearing all popups.
        /// </summary>
        public void BreakPopupFlow()
        {
            _model?.OnClose?.Invoke(_popupName);
            Dispose();
            _builder.ClearPopups();
        }
    }
}