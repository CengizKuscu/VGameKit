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
    /// Manages the lifecycle and display logic for rewarded ads.
    /// Handles loading, showing, retrying, and event callbacks for ad interactions and reward granting.
    /// </summary>
    public class RewardedAds : AdsBaseView<RewardedAd, AdsItemModel>
    {
        /// <summary>
        /// The current ad event being processed.
        /// </summary>
        private AdsEvent _currentAdsEvent;

        /// <summary>
        /// Event triggered when the ready status of the rewarded ad changes.
        /// </summary>
        public static event Action<bool> onRewardedReadyStatusChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="RewardedAds"/> class.
        /// </summary>
        /// <param name="adsItemModel">The ad item model containing ad configuration.</param>
        public RewardedAds(AdsItemModel adsItemModel) : base(adsItemModel)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the rewarded ad is loaded and can be shown.
        /// </summary>
        public override bool IsLoaded => AdsItem?.CanShowAd() ?? false;

        /// <summary>
        /// Loads a rewarded ad by requesting a new ad.
        /// </summary>
        public override void LoadAd()
        {
            RequestAd();
        }

        /// <summary>
        /// Requests a new rewarded ad and destroys any existing ad instance.
        /// </summary>
        public override void RequestAd()
        {
            //_currentAdsEvent = null;
            DestroyAd();

            var adRequest = new AdRequest();
            RewardedAd.Load(AdsItemModel.AdsId, adRequest, HandleOnAdLoaded);
        }

        /// <summary>
        /// Handles the callback when an ad is loaded or fails to load.
        /// </summary>
        /// <param name="arg1">The loaded rewarded ad instance.</param>
        /// <param name="arg2">The error if loading failed.</param>
        private void HandleOnAdLoaded(RewardedAd arg1, LoadAdError arg2)
        {
            if (arg2 is not null)
            {
                _isRetryRequestAd = false;
#if GA_ENABLED
                GameAnalytics.NewDesignEvent("RewardedAd:FailToLoad");
#endif
                Debug.Log("RewardedAd FAILED TO LOAD");
                onResponseAdEvent?.Invoke(new AdsEvent(AdsItemModel.AdsType), AdsEventStatus.ResponseLoadFail);
                onRewardedReadyStatusChanged?.Invoke(false);
                AdsItem = null;
                RequestAdAsync().Forget();
                return;
            }
            else if (arg1 is null)
            {
                _isRetryRequestAd = false;
#if GA_ENABLED
                GameAnalytics.NewDesignEvent("RewardedAd:FailToLoad");
#endif
                Debug.Log("RewardedAd FAILED TO LOAD");
                onResponseAdEvent?.Invoke(new AdsEvent(AdsItemModel.AdsType), AdsEventStatus.ResponseLoadFail);
                onRewardedReadyStatusChanged?.Invoke(false);
                AdsItem = null;
                RequestAdAsync().Forget();
                return;
            }

            AdsItem = arg1;
            AddListeners();
            Debug.Log("RewardedAd Loaded");
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
        /// Shows the rewarded ad for the given ad event.
        /// </summary>
        /// <param name="e">The ad event to associate with the show action.</param>
        public void ShowAd(AdsEvent e)
        {
            _currentAdsEvent = e;
            ShowAd();
        }

        /// <summary>
        /// Shows the rewarded ad if it is ready.
        /// Invokes appropriate callbacks if not ready or if reward is earned.
        /// </summary>
        public override void ShowAd()
        {
            if (AdsItem is null)
            {
                onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.NotRequestedYet);
                return;
            }

            if (IsLoaded)
            {
                AdsItem.Show((Reward reward) =>
                {
                    _currentAdsEvent.IsEarnedReward = true;
                    onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.ResponseEarnReward);
                });
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
            if (AdsItem != null)
            {
                RemoveListeners();
                AdsItem.Destroy();
                AdsItem = null;
            }
        }

        /// <summary>
        /// Adds event listeners to the current ad instance and notifies ready status.
        /// </summary>
        public override void AddListeners()
        {
            if (AdsItem is not null)
            {
                AdsItem.OnAdFullScreenContentClosed += HandleOnAdFullScreenContentClosed;
                AdsItem.OnAdFullScreenContentOpened += HandleOnAdFullScreenContentOpened;
                AdsItem.OnAdFullScreenContentFailed += HandleOnAdFullScreenContentFailed;
                onRewardedReadyStatusChanged?.Invoke(true);
            }
        }

        /// <summary>
        /// Removes event listeners from the current ad instance and clears ready status event.
        /// </summary>
        public override void RemoveListeners()
        {
            if (AdsItem is not null)
            {
                AdsItem.OnAdFullScreenContentClosed -= HandleOnAdFullScreenContentClosed;
                AdsItem.OnAdFullScreenContentOpened -= HandleOnAdFullScreenContentOpened;
                AdsItem.OnAdFullScreenContentFailed -= HandleOnAdFullScreenContentFailed;
                onRewardedReadyStatusChanged = null;
            }
        }

        /// <summary>
        /// Handles the event when the ad is closed by the user.
        /// Invokes the closed event and loads a new ad.
        /// </summary>
        private void HandleOnAdFullScreenContentClosed()
        {
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
            if (string.IsNullOrEmpty(_currentAdsEvent.From))
            {
                GameAnalytics.NewDesignEvent("RewardedAd:Showed");
            }
            else
            {
                GameAnalytics.NewDesignEvent($"RewardedAd:Showed:{_currentAdsEvent.From}");
            }

            onResponseAdEvent?.Invoke(_currentAdsEvent, AdsEventStatus.ResponseOpened);
#endif
        }

        /// <summary>
        /// Handles the event when the ad fails to show.
        /// Invokes the show fail event and loads a new ad.
        /// </summary>
        /// <param name="error">The error that occurred while showing the ad.</param>
        private void HandleOnAdFullScreenContentFailed(AdError error)
        {
#if GA_ENABLED
            if (string.IsNullOrEmpty(_currentAdsEvent.From))
            {
                GameAnalytics.NewDesignEvent("RewardedAd:FailToShow");
            }
            else
            {
                GameAnalytics.NewDesignEvent($"RewardedAd:FailToShow:{_currentAdsEvent.From}");
            }
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
}