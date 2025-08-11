using System;
using Cysharp.Threading.Tasks;
#if GA_ENABLED
using GameAnalyticsSDK;
#endif
using GoogleMobileAds.Api;
using UnityEngine;
using VGameKit.GoogleAds.Runtime.Events;

namespace VGameKit.GoogleAds.Runtime
{
    /// <summary>
    /// Manages the lifecycle and display logic for interstitial ads.
    /// Handles loading, showing, retrying, and event callbacks for ad interactions.
    /// </summary>
    public class InterstitialAds : AdsBaseView<InterstitialAd, InterstitialAdsItemModel>
    {
        /// <summary>
        /// The current ad event being processed.
        /// </summary>
        private AdsEvent _currentAdsEvent;

        /// <summary>
        /// The time when the last ad request was made.
        /// </summary>
        public float RequestTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterstitialAds"/> class.
        /// </summary>
        /// <param name="adsItemModel">The ad item model containing ad configuration.</param>
        public InterstitialAds(InterstitialAdsItemModel adsItemModel) : base(adsItemModel)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the interstitial ad is loaded and can be shown.
        /// </summary>
        public override bool IsLoaded => AdsItem?.CanShowAd() ?? false;

        /// <summary>
        /// Loads an interstitial ad by requesting a new ad.
        /// </summary>
        public override void LoadAd()
        {
            RequestAd();
        }

        /// <summary>
        /// Requests a new interstitial ad and destroys any existing ad instance.
        /// </summary>
        public override void RequestAd()
        {
            _currentAdsEvent = null;
            DestroyAd();

            var adRequest = new AdRequest();
            InterstitialAd.Load(AdsItemModel.AdsId, adRequest, HandleOnAdLoaded);
        }

        /// <summary>
        /// Handles the callback when an ad is loaded or fails to load.
        /// </summary>
        /// <param name="ad">The loaded interstitial ad instance.</param>
        /// <param name="error">The error if loading failed.</param>
        private void HandleOnAdLoaded(InterstitialAd ad, LoadAdError error)
        {
            if (error is not null)
            {
                _isRetryRequestAd = false;
#if GA_ENABLED
                GameAnalytics.NewDesignEvent("InterstitialAd:FailToLoad");
#endif
                Debug.Log("InterstitialAd FAILED TO LOAD");
                onResponseAdEvent?.Invoke(new AdsEvent(AdsItemModel.AdsType), AdsEventStatus.ResponseLoadFail);
                AdsItem = null;
                RequestAdAsync().Forget();
                return;
            }
            else if (ad is null)
            {
                _isRetryRequestAd = false;
#if GA_ENABLED
                GameAnalytics.NewDesignEvent("InterstitialAd:FailToLoad");
#endif
                Debug.Log("InterstitialAd FAILED TO LOAD");
                onResponseAdEvent?.Invoke(new AdsEvent(AdsItemModel.AdsType), AdsEventStatus.ResponseLoadFail);
                AdsItem = null;
                RequestAdAsync().Forget();
                return;
            }

            AdsItem = ad;
            RequestTime = Time.time;
            AddListeners();
            Debug.Log("InterstitialAd Loaded");
        }

        /// <summary>
        /// Indicates whether a retry request for an ad is in progress.
        /// </summary>
        private bool _isRetryRequestAd;

        /// <summary>
        /// Asynchronously retries requesting an ad after a delay if loading fails.
        /// </summary>
        private async UniTask RequestAdAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(35));
            try
            {
                if (!_isRetryRequestAd || AdsItem is null)
                {
                    _isRetryRequestAd = true;
                    RequestAd();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Shows the interstitial ad for the given ad event.
        /// </summary>
        /// <param name="e">The ad event to associate with the show action.</param>
        public void ShowAd(AdsEvent e)
        {
            _currentAdsEvent = e;
            ShowAd();
        }

        /// <summary>
        /// Shows the interstitial ad if it is ready and meets timing requirements.
        /// Invokes appropriate callbacks if not ready.
        /// </summary>
        public override void ShowAd()
        {
            if (AdsItem is null)
            {
                onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.NotRequestedYet);
                return;
            }

            var diffTime = Time.time - RequestTime;
            if (diffTime >= AdsItemModel.Duration || AdsItemModel.UseAutoShow)
            {
                if (AdsItem is not null && AdsItem.CanShowAd())
                {
                    AdsItem.Show();
                }
                else
                {
                    onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.NotReadyYet);
                }
            }
            else
            {
                onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.NotReadyYet);
            }
        }

        /// <summary>
        /// Destroys the current ad and removes event listeners.
        /// </summary>
        public override void DestroyAd()
        {
            if (AdsItem is not null)
            {
                RemoveListeners();
                AdsItem.Destroy();
                AdsItem = null;
            }
        }

        /// <summary>
        /// Adds event listeners to the current ad instance.
        /// </summary>
        public override void AddListeners()
        {
            if (AdsItem is not null)
            {
                AdsItem.OnAdFullScreenContentClosed += HandleOnAdFullScreenContentClosed;
                AdsItem.OnAdFullScreenContentOpened += HandleOnAdFullScreenContentOpened;
                AdsItem.OnAdFullScreenContentFailed += HandleOnAdFullScreenContentFailed;
            }
        }

        /// <summary>
        /// Removes event listeners from the current ad instance.
        /// </summary>
        public override void RemoveListeners()
        {
            if (AdsItem is not null)
            {
                AdsItem.OnAdFullScreenContentClosed -= HandleOnAdFullScreenContentClosed;
                AdsItem.OnAdFullScreenContentOpened -= HandleOnAdFullScreenContentOpened;
                AdsItem.OnAdFullScreenContentFailed -= HandleOnAdFullScreenContentFailed;
            }
        }

        /// <summary>
        /// Handles the event when the ad is closed by the user.
        /// Invokes the closed event and loads a new ad.
        /// </summary>
        private void HandleOnAdFullScreenContentClosed()
        {
            //GameAnalytics.NewDesignEvent("InterstitialAd:Closed");
            onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.ResponseClosed);
            LoadAd();
        }

        /// <summary>
        /// Handles the event when the ad is opened (shown) to the user.
        /// Invokes the opened event and logs analytics.
        /// </summary>
        private void HandleOnAdFullScreenContentOpened()
        {
#if GA_ENABLED
            GameAnalytics.NewDesignEvent("InterstitialAd:Showed");
#endif
            onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.ResponseOpened);
        }

        /// <summary>
        /// Handles the event when the ad fails to show.
        /// Invokes the show fail event and loads a new ad.
        /// </summary>
        /// <param name="error">The error that occurred while showing the ad.</param>
        private void HandleOnAdFullScreenContentFailed(AdError error)
        {
#if GA_ENABLED
            GameAnalytics.NewDesignEvent("InterstitialAd:FailToShow");
#endif
            onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.ResponseShowFail);
            LoadAd();
        }

        /// <summary>
        /// Disposes the ad by destroying it and cleaning up resources.
        /// </summary>
        public override void Dispose()
        {
            DestroyAd();
        }
    }

    /// <summary>
    /// Model for interstitial ad configuration, including auto-show option.
    /// </summary>
    public class InterstitialAdsItemModel : AdsItemModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the ad should auto-show when ready.
        /// </summary>
        public bool UseAutoShow { get; set; }
    }
}