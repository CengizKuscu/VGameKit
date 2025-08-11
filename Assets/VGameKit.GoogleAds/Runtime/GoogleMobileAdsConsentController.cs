using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace VGameKit.GoogleAds.Runtime
{
    /// <summary>
    /// Represents the custom consent status for Google Mobile Ads.
    /// </summary>
    public enum CustomConsentStatus
    {
        None,
        Failed,
        Unknown,
        NotRequired,
        Required,
        Obtained,
        ShowForm
    }

    /// <summary>
    /// Controls the consent flow for Google Mobile Ads using the User Messaging Platform (UMP).
    /// Handles consent gathering, privacy form display, and privacy button state.
    /// </summary>
    public class GoogleMobileAdsConsentController
    {
        private string _testDeviceId;

        private static CustomConsentStatus _initializeResult = CustomConsentStatus.None;

        private static bool _privacyBtnInteractable = false;

        /// <summary>
        /// Gets whether the privacy button should be interactable based on consent requirements.
        /// </summary>
        public static bool PrivacyBtnInteractable => _privacyBtnInteractable;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleMobileAdsConsentController"/> class.
        /// </summary>
        /// <param name="testDeviceId">The test device ID for debugging consent flow.</param>
        public GoogleMobileAdsConsentController(string testDeviceId)
        {
            _testDeviceId = testDeviceId;
        }

        /// <summary>
        /// Gets a value indicating whether ads can be requested based on the current consent status.
        /// </summary>
        public bool CanRequestAds
        {
            get
            {
                UpdatePrivacyButton();
                return ConsentInformation.ConsentStatus == ConsentStatus.Obtained ||
                       ConsentInformation.ConsentStatus == ConsentStatus.NotRequired;
            }
        }

        /// <summary>
        /// Updates the privacy button's interactable state based on the privacy options requirement status.
        /// </summary>
        private static void UpdatePrivacyButton()
        {
            _privacyBtnInteractable =
                ConsentInformation.PrivacyOptionsRequirementStatus ==
                PrivacyOptionsRequirementStatus.Required;
        }

        /// <summary>
        /// Asynchronously shows the privacy form if required.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>True if the form was shown successfully; otherwise, false.</returns>
        public static async UniTask<bool> ShowPrivacyForm(CancellationToken token)
        {
            bool? result = null;

            ConsentForm.LoadAndShowConsentFormIfRequired(showError =>
            {
                UpdatePrivacyButton();
                if (showError != null)
                {
                    Debug.LogError($"Failed to show consent form: {showError}");
                    result = false;
                }
                else
                {
                    result = true;
                }
            });

            if (!result.HasValue)
            {
                await UniTask.WaitUntil(() => result.HasValue, cancellationToken: token);
            }

            return result.HasValue ? result.Value : false;
        }

        /// <summary>
        /// Asynchronously gathers user consent for ads, showing the consent form if necessary.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>The resulting <see cref="CustomConsentStatus"/> after attempting to gather consent.</returns>
        public async UniTask<CustomConsentStatus> GatherConsent(CancellationToken token)
        {
#if GOOGLEADS_TESTDEVICE
                    Debug.unityLogger.logEnabled = true;
                    ConsentInformation.Reset();
                    var requestParameters = new ConsentRequestParameters
                    {
                        TagForUnderAgeOfConsent = false,
                        ConsentDebugSettings = new ConsentDebugSettings
                        {
                            DebugGeography = DebugGeography.EEA
                        }
                    };
                    if (string.IsNullOrEmpty(_testDeviceId))
                    {
                        requestParameters.ConsentDebugSettings.TestDeviceHashedIds = new List<string>
                        {
                            AdRequest.TestDeviceSimulator,
                        };
                    }
                    else
                    {
                        requestParameters.ConsentDebugSettings.TestDeviceHashedIds = new List<string>
                        {
                            AdRequest.TestDeviceSimulator,
                            _testDeviceId
                        };
                    }
#else
            var requestParameters = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false
            };
#endif

            _initializeResult = CustomConsentStatus.None;

            // The Google Mobile Ads SDK provides the User Messaging Platform (Google's
            // IAB Certified consent management platform) as one solution to capture
            // consent for users in GDPR impacted countries. This is an example and
            // you can choose another consent management platform to capture consent.
            ConsentInformation.Update(requestParameters, error =>
            {
                UpdatePrivacyButton();

                if (error != null)
                {
                    UnityEngine.Debug.Log(
                        $"##### GoogleAds Consent failed ErrorCode: {error.ErrorCode} ErrorMessage: {error.Message}");
                    _initializeResult = CustomConsentStatus.Failed;
                    return;
                }

                if (CanRequestAds)
                {
                    Debug.Log(
                        $"##### Consent has already been gathered or not required. ConsentStatus is {ConsentInformation.ConsentStatus}");
                    // Consent has already been gathered or not required.
                    // Return control back to the user.
                    _initializeResult = (ConsentStatus.Obtained == ConsentInformation.ConsentStatus)
                        ? CustomConsentStatus.Obtained
                        : CustomConsentStatus.NotRequired;
                    return;
                }

                // Consent not obtained and is required.
                // Load the initial consent request form for the user.
                ConsentForm.LoadAndShowConsentFormIfRequired(showError =>
                {
                    UpdatePrivacyButton();
                    if (showError != null)
                    {
                        Debug.Log(
                            $"##### Show ConsentForm failed. ErrorCode: {showError.ErrorCode} ErrorMessage: {showError.Message}");
                        _initializeResult = CustomConsentStatus.Failed;
                    }
                    else
                    {
                        Debug.Log(
                            $"##### Show ConsentForm succeeded. ConsentStatus is {ConsentInformation.ConsentStatus}");
                        _initializeResult = CustomConsentStatus.ShowForm;
                    }
                });
            });

            await UniTask.WaitUntil(() => _initializeResult == CustomConsentStatus.None, cancellationToken: token)
                .AttachExternalCancellation(token).SuppressCancellationThrow();

            Debug.Log($"##### GoogleAds Consent Initialize Complete");

            return _initializeResult;
        }
    }
}