using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace VGameKit.Runtime.UI.Popup
{
    /// <summary>
    /// Base class for building and managing popups.
    /// Handles popup queueing, instantiation, and disposal.
    /// </summary>
    /// <typeparam name="TPopupName">Enum type representing popup names.</typeparam>
    public class BasePopupBuilder<TPopupName> : MonoBehaviour, IDisposable where TPopupName : Enum
    {
        /// <summary>
        /// Root transform for popups.
        /// </summary>
        [SerializeField] protected RectTransform _popupRoot;
    
        /// <summary>
        /// List of popup prefabs.
        /// </summary>
        [SerializeField] protected List<BasePopup<TPopupName>> _prefabs;
    
        /// <summary>
        /// Queue of popup data to be displayed.
        /// </summary>
        protected Queue<(TPopupName name, BasePopupModel<TPopupName> model)> _popupDataQueue = new();
    
        /// <summary>
        /// Currently active popup.
        /// </summary>
        protected BasePopup<TPopupName> _currentPopup;
    
        /// <summary>
        /// Action to invoke when popup flow is complete.
        /// </summary>
        protected Action _onCompleteFlow;
    
        /// <summary>
        /// Dependency injection resolver for instantiating popups.
        /// </summary>
        [Inject] protected readonly IObjectResolver _resolver;
    
        /// <summary>
        /// Sets the completion callback for the popup flow.
        /// </summary>
        /// <param name="onCompleteFlow">Action to invoke on completion.</param>
        /// <returns>Self for chaining.</returns>
        public BasePopupBuilder<TPopupName> OnCompleteFlow(Action onCompleteFlow)
        {
            var completeFlow = _onCompleteFlow;
            if (completeFlow != null)
            {
                _onCompleteFlow = null;
            }
    
            _onCompleteFlow = onCompleteFlow;
    
            return this;
        }
    
        /// <summary>
        /// Clears all popups and invokes the completion callback.
        /// </summary>
        public void ClearPopups()
        {
            if (_currentPopup != null)
            {
                try
                {
                    _currentPopup.Dispose();
                }
                catch (Exception)
                {
                    //Debug.LogError($"[AbsPopupBuilder] ClearPopups: Exception when disposing popup {_currentPopup.Name}. Exception: {e}");
                }
    
                _currentPopup = null;
            }
    
            if (_popupDataQueue.Any())
            {
                _popupDataQueue.Clear();
            }
    
            _onCompleteFlow?.Invoke();
            _onCompleteFlow = null;
        }
    
        /// <summary>
        /// Adds a popup to the queue.
        /// </summary>
        /// <typeparam name="TModel">Type of the popup model.</typeparam>
        /// <param name="name">Popup name.</param>
        /// <param name="model">Popup model.</param>
        /// <returns>Self for chaining.</returns>
        public BasePopupBuilder<TPopupName> AddPopup<TModel>(TPopupName name, TModel model)
            where TModel : BasePopupModel<TPopupName>
        {
            var prefab = _prefabs.Find(s => s.Name.Equals(name));
    
            if (prefab == null)
            {
                Debug.LogError($"[AbsPopupBuilder] AddPopup: Prefab with name {name} not found.");
                return this;
            }
    
            _popupDataQueue.Enqueue((name: name, model: model));
    
            return this;
        }
    
        /// <summary>
        /// Opens the next popup in the queue.
        /// </summary>
        public void OpenPopup()
        {
            if (!_popupDataQueue.Any())
            {
                ClearPopups();
                return;
            }
    
            var queueTuple = _popupDataQueue.Peek();
    
            try
            {
                var prefab = _prefabs.Find(s => s.Name.Equals(queueTuple.name));
    
                if (prefab == null)
                {
                    return;
                }
    
                var popup = _resolver.Instantiate(prefab, _popupRoot.transform);
                popup.transform.SetAsLastSibling();
                popup.transform.localScale = Vector3.one;
                popup.SetBuilder(this);
                popup.Init(queueTuple.model);
    
                _popupDataQueue.Dequeue();
                _currentPopup = popup;
                popup.Open();
            }
            catch (Exception)
            {
                Debug.LogError($"OpenPopup: Failed to create popup: {queueTuple.name}");
            }
        }
    
        /// <summary>
        /// Closes the specified popup and opens the next one in the queue.
        /// </summary>
        /// <param name="popup">Popup to close.</param>
        public void ClosePopup(BasePopup<TPopupName> popup)
        {
            popup.Dispose();
            _currentPopup = null;
            OpenPopup();
        }
    
        /// <summary>
        /// Disposes the popup builder and clears all popups.
        /// </summary>
        public void Dispose()
        {
            ClearPopups();
        }
    }
}