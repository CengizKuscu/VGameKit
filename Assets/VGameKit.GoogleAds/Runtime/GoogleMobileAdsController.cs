using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using MessagePipe;
using UnityEngine;
using VContainer;
using VGameKit.GoogleAds.Runtime.Events;

namespace VGameKit.GoogleAds.Runtime
{
    /// <summary>
    /// Represents the custom initialization status for Google Mobile Ads.
    /// </summary>
    public enum CustomInitializeStatus
    {
        None,
        NotInitialized,
        Success,
        Failed
    }

    /// <summary>
    /// Controls the initialization and management of Google Mobile Ads within the application.
    /// Handles consent, ad preparation, and ad event subscriptions.
    /// </summary>
    public class GoogleMobileAdsController : MonoBehaviour, IDisposable
    {
        [SerializeField] private bool _useConsent = true;
        [SerializeField] private bool _useTestAds = false;

        [Space(5f)] [Header("Banner")] [SerializeField]
        private bool _useBanner = true;

        [SerializeField] private int _bannerReloadMinutes = 5;

        [Space(5f)] [Header("Interstitial")] [SerializeField]
        private bool _useInterstitial = true;

        [SerializeField] private int _interstitialDurationSeconds = 45;
        [SerializeField] private bool _interstitialAutoShow = false;

        [Space(5f)] [Header("Rewarded")] [SerializeField]
        private bool _useRewarded = false;

        [Space(5f)]
        [Header(
            "Test Device: \nCopy your test device ID from the XCODE logs and paste it here.\nUMPDebugSettings.testDeviceIdentifiers = @[ @\"D01738DA-B\" ];")]
        [SerializeField]
        private string _testDeviceIdIOS = "";

        [SerializeField] private string _testDeviceIdDRD = "";

        [Space(5f)] [SerializeField] private AdsIds _googleAdsIds;

        private GoogleMobileAdsConsentController _consentController;

        private RequestConfiguration _requestConfiguration;

        private CustomInitializeStatus _initializeResult = CustomInitializeStatus.NotInitialized;
        private CustomInitializeStatus _consentInitializeResult = CustomInitializeStatus.NotInitialized;

        /// <summary>
        /// Gets the result of the Google Mobile Ads initialization.
        /// </summary>
        public CustomInitializeStatus InitializeResult => _initializeResult;

        /// <summary>
        /// Gets the result of the consent initialization.
        /// </summary>
        public CustomInitializeStatus ConsentInitializeResult => _consentInitializeResult;

        private IDisposable _subscription;
        private DisposableBagBuilder _bagBuilder;

        private ISubscriber<AdsEventStatus, AdsEvent> _adsEventSubscriber;
        private IPublisher<AdsEventStatus, AdsEvent> _adsEventPublisher;

        private BannerAds _bannerAds;
        private static InterstitialAds _interstitialAds;
        private static RewardedAds _rewardedAds;

        /// <summary>
        /// Gets a value indicating whether the interstitial ad is ready to be shown.
        /// </summary>
        public static bool IsInterstitialAdReady => _interstitialAds?.IsLoaded ?? false;

        /// <summary>
        /// Gets a value indicating whether the rewarded ad is ready to be shown.
        /// </summary>
        public static bool IsRewardedAdReady => _rewardedAds?.IsLoaded ?? false;

        /// <summary>
        /// Injects dependencies and sets up ad event subscriptions.
        /// </summary>
        /// <param name="resolver">The dependency resolver.</param>
        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _adsEventSubscriber = resolver.Resolve<ISubscriber<AdsEventStatus, AdsEvent>>();
            _adsEventPublisher = resolver.Resolve<IPublisher<AdsEventStatus, AdsEvent>>();

            AddSubscriptions();
        }

        /// <summary>
        /// Adds subscriptions to ad events.
        /// </summary>
        private void AddSubscriptions()
        {
            _bagBuilder = DisposableBag.CreateBuilder();
            _adsEventSubscriber.Subscribe(AdsEventStatus.Request, @event => OnAdsRequest(@event)).AddTo(_bagBuilder);
            _subscription = _bagBuilder.Build();
        }

        /// <summary>
        /// Handles ad requests based on the ad type.
        /// </summary>
        /// <param name="e">The ad event.</param>
        private void OnAdsRequest(AdsEvent e)
        {
            switch (e.AdsType)
            {
                case AdsType.Banner:
                    if (_useBanner)
                    {
                        _bannerAds?.ShowAd();
                    }

                    break;
                case AdsType.Interstitial:
                    if (_useInterstitial)
                    {
                        _interstitialAds.ShowAd(e);
                    }

                    break;
                case AdsType.Rewarded:
                    if (_useRewarded)
                    {
                        _rewardedAds.ShowAd(e);
                    }

                    break;
            }
        }

        /// <summary>
        /// Initializes Google Mobile Ads and prepares ads based on configuration.
        /// Handles consent flow if enabled.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        public async UniTask Initialize(CancellationToken token)
        {
            if (_requestConfiguration != null)
            {
                return;
            }

            string testDeviceId = "";

#if UNITY_IOS
            testDeviceId = _testDeviceIdIOS;
#elif UNITY_ANDROID
            testDeviceId = _testDeviceIdDRD;
#endif

            _consentController ??= new GoogleMobileAdsConsentController(testDeviceId);

            // On Android, Unity is paused when displaying interstitial or rewarded video.
            // This setting makes iOS behave consistently with Android.
            MobileAds.SetiOSAppPauseOnBackground(true);

            // When true all events raised by GoogleMobileAds will be raised
            // on the Unity main thread. The default value is false.
            // https://developers.google.com/admob/unity/quick-start#raise_ad_events_on_the_unity_main_thread
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            // Configure your RequestConfiguration with Child Directed Treatment
            // and the Test Device Ids.
            _requestConfiguration = new RequestConfiguration()
            {
                TagForChildDirectedTreatment = TagForChildDirectedTreatment.Unspecified,
                TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.Unspecified
            };
            MobileAds.SetRequestConfiguration(_requestConfiguration);

            if (_useConsent)
            {
                if (_consentController.CanRequestAds)
                {
                    await InitializeGoogleMobileAds(token).AttachExternalCancellation(token)
                        .SuppressCancellationThrow();
                }
                else
                {
                    var consentResult = await _consentController.GatherConsent(token).AttachExternalCancellation(token)
                        .SuppressCancellationThrow();

                    await InitializeGoogleMobileAds(token).AttachExternalCancellation(token)
                        .SuppressCancellationThrow();
                }
            }
            else
            {
                await InitializeGoogleMobileAds(token).AttachExternalCancellation(token).SuppressCancellationThrow();
            }

            await UniTask.DelayFrame(5, cancellationToken: token);

            PrepareAds();
        }

        /// <summary>
        /// Initializes the Google Mobile Ads SDK asynchronously.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        private async UniTask InitializeGoogleMobileAds(CancellationToken token)
        {
            if (_initializeResult != CustomInitializeStatus.NotInitialized) return;

            _initializeResult = CustomInitializeStatus.None;

            Debug.Log("##### Google Mobile Ads Initializing.");
            MobileAds.Initialize((InitializationStatus initstatus) =>
            {
                if (initstatus == null)
                {
                    Debug.LogError("##### Google Mobile Ads initialization failed.");
                    _initializeResult = CustomInitializeStatus.Failed;
                    return;
                }

                // If you use mediation, you can check the status of each adapter.
                var adapterStatusMap = initstatus.getAdapterStatusMap();
                if (adapterStatusMap != null)
                {
                    foreach (var item in adapterStatusMap)
                    {
                        Debug.Log(string.Format("Adapter {0} is {1}",
                            item.Key,
                            item.Value.InitializationState));
                    }
                }

                Debug.Log("##### Google Mobile Ads initialization is Success.");
                _initializeResult = CustomInitializeStatus.Success;
            });

            await UniTask.WaitWhile(() => _initializeResult == CustomInitializeStatus.None, cancellationToken: token);
        }

        /// <summary>
        /// Prepares all enabled ad types (banner, interstitial, rewarded).
        /// </summary>
        private void PrepareAds()
        {
            if (_useBanner)
            {
                PrepareBannerAds();
            }

            if (_useInterstitial)
            {
                PrepareInterstitialAds();
            }

            if (_useRewarded)
            {
                PrepareRewardedAds();
            }
        }

        /// <summary>
        /// Prepares rewarded ads and sets up event handlers.
        /// </summary>
        private void PrepareRewardedAds()
        {
            RemoveRewardedAdsHandlers();
            var rewardedId = _googleAdsIds.GetRewardedId(_useTestAds);
            var adsModel = new AdsItemModel
            {
                AdsId = rewardedId,
                AdsType = AdsType.Rewarded,
                Duration = 0
            };

            _rewardedAds ??= new RewardedAds(adsModel);

            AddRewardedAdsHandlers();
            _rewardedAds.LoadAd();
            Debug.Log("##### Prepare Rewarded Ads Complete");
        }

        /// <summary>
        /// Adds event handlers for rewarded ads.
        /// </summary>
        private void AddRewardedAdsHandlers()
        {
            if (_rewardedAds is null) return;
            _rewardedAds.onResponseAdEvent += OnRewardedAdsResponseAdEvent;
        }

        /// <summary>
        /// Removes event handlers for rewarded ads.
        /// </summary>
        private void RemoveRewardedAdsHandlers()
        {
            if (_rewardedAds is null) return;
            _rewardedAds.onResponseAdEvent -= OnRewardedAdsResponseAdEvent;
        }

        /// <summary>
        /// Handles rewarded ad response events and publishes them.
        /// </summary>
        /// <param name="eAdsEvent">The ad event.</param>
        /// <param name="eventStatus">The status of the ad event.</param>
        private void OnRewardedAdsResponseAdEvent(AdsEvent eAdsEvent, AdsEventStatus eventStatus)
        {
            if (eAdsEvent.AdsType == AdsType.Rewarded && eventStatus == AdsEventStatus.ResponseClosed)
            {
                if (_useInterstitial && _interstitialAds is not null)
                    _interstitialAds.RequestTime = Time.time;
            }

            _adsEventPublisher.Publish(eventStatus, eAdsEvent);
        }

        /// <summary>
        /// Prepares interstitial ads and sets up event handlers.
        /// </summary>
        private void PrepareInterstitialAds()
        {
            RemoveInterstitialAdsHandlers();
            var interstitialId = _googleAdsIds.GetInterstitialId(_useTestAds);
            var adsModel = new InterstitialAdsItemModel
            {
                AdsId = interstitialId,
                AdsType = AdsType.Interstitial,
                Duration = _interstitialDurationSeconds,
                UseAutoShow = _interstitialAutoShow
            };
            _interstitialAds ??= new InterstitialAds(adsModel);

            AddInterstitialAdsHandlers();

            _interstitialAds.LoadAd();
            Debug.Log("##### Prepare Interstitial Ads Complete");
        }

        /// <summary>
        /// Removes event handlers for interstitial ads.
        /// </summary>
        private void RemoveInterstitialAdsHandlers()
        {
            if (_interstitialAds is null) return;
            _interstitialAds.onResponseAdEvent -= OnInterstitialAdsResponseAdEvent;
        }

        /// <summary>
        /// Adds event handlers for interstitial ads.
        /// </summary>
        private void AddInterstitialAdsHandlers()
        {
            if (_interstitialAds is null) return;
            _interstitialAds.onResponseAdEvent += OnInterstitialAdsResponseAdEvent;
        }

        /// <summary>
        /// Handles interstitial ad response events and publishes them.
        /// </summary>
        /// <param name="eAdsEvent">The ad event.</param>
        /// <param name="eventStatus">The status of the ad event.</param>
        private void OnInterstitialAdsResponseAdEvent(AdsEvent eAdsEvent, AdsEventStatus eventStatus)
        {
            _adsEventPublisher.Publish(eventStatus, eAdsEvent);
        }

        /// <summary>
        /// Prepares banner ads for display.
        /// </summary>
        private void PrepareBannerAds()
        {
            var bannerId = _googleAdsIds.GetBannerId(_useTestAds);
            var adsModel = new AdsItemModel
            {
                AdsId = bannerId,
                AdsType = AdsType.Banner,
                Duration = _bannerReloadMinutes
            };
            _bannerAds ??= new BannerAds(adsModel);
            _bannerAds.RequestAd();
            Debug.Log("##### Prepare Banner Ads Complete");
        }

        /// <summary>
        /// Disposes of the Google Mobile Ads controller and its resources.
        /// This includes disposing of banner, interstitial, and rewarded ads, as well as the
        /// subscription to ad events.
        /// </summary>
        public void Dispose()
        {
            _bannerAds?.Dispose();
            _interstitialAds?.Dispose();
            _rewardedAds?.Dispose();
            _subscription?.Dispose();
        }
    }
}