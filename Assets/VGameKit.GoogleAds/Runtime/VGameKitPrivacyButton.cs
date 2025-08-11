using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using UnityEngine.UI;

namespace VGameKit.GoogleAds.Runtime
{
    /// <summary>
    /// Handles the privacy button UI for Google Mobile Ads consent.
    /// Manages button state, click events, and displays the privacy form when required.
    /// </summary>
    public class VGameKitPrivacyButton : MonoBehaviour
    {
        /// <summary>
        /// Reference to the UI Button component.
        /// </summary>
        private Button _btn;
    
        /// <summary>
        /// Cancellation token source for async operations.
        /// </summary>
        private CancellationTokenSource _cts;
    
        /// <summary>
        /// Initializes the button and sets its interactable state based on consent requirements.
        /// </summary>
        private void Awake()
        {
            bool btnShow = false;
            TryGetComponent(out _btn);
    
            if (_btn is not null)
            {
                btnShow = GoogleMobileAdsConsentController.PrivacyBtnInteractable;
    
                _btn.interactable = btnShow;
                gameObject.SetActive(btnShow);
            }
        }
    
        /// <summary>
        /// Cleans up listeners and cancels any ongoing operations when the object is disabled.
        /// </summary>
        private void OnDisable()
        {
            _cts?.Cancel();
            if (_btn is not null)
            {
                _btn.onClick.RemoveAllListeners();
            }
        }
    
        /// <summary>
        /// Sets up the button click listener and cancels any previous operations when enabled.
        /// </summary>
        private void OnEnable()
        {
            _cts?.Cancel();
            if (_btn is not null)
            {
                _btn.onClick.AddListener(() =>
                {
                    _cts = new CancellationTokenSource();
                    OnClick(_cts.Token).SuppressCancellationThrow();
                });
            }
        }
    
        /// <summary>
        /// Handles the button click event, shows the privacy form, and updates button state.
        /// </summary>
        /// <param name="token">Cancellation token for the async operation.</param>
        private async UniTask OnClick(CancellationToken token)
        {
            _btn.interactable = false;
            bool result = false;
    
            await GoogleMobileAdsConsentController.ShowPrivacyForm(token).AttachExternalCancellation(token)
                .SuppressCancellationThrow();
    
            var btnShow = ConsentInformation.PrivacyOptionsRequirementStatus ==
                          PrivacyOptionsRequirementStatus.Required;
    
            gameObject.SetActive(btnShow);
            _btn.interactable = btnShow;
    
            _cts.Cancel();
        }
    }
}