using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;

namespace VGameKit.GoogleAds.Runtime
{
    /// <summary>
    /// Manages the lifecycle and display of banner ads using Google Mobile Ads.
    /// Handles loading, showing, hiding, and reloading banners at specified intervals.
    /// </summary>
    public class BannerAds : AdsBaseView<BannerView, AdsItemModel>
    {
        /// <summary>
        /// Indicates whether the banner ad is currently being shown.
        /// </summary>
        private bool _isShowing = false;
    
        /// <summary>
        /// Token source for cancelling the reload timer.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="BannerAds"/> class.
        /// </summary>
        /// <param name="adsItemModel">The ad item model containing ad configuration.</param>
        public BannerAds(AdsItemModel adsItemModel) : base(adsItemModel)
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }
        
        /// <summary>
        /// Requests a new banner ad. Destroys any existing banner before creating a new one.
        /// </summary>
        public override void RequestAd()
        {
            // If we already have a banner, destroy the old one.
            DestroyAd();
    
            AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
    
            AdsItem = null;
    
            AdsItem = new BannerView(AdsItemModel.AdsId, adaptiveSize, AdPosition.Bottom);
    
            // Listen to events the banner may raise.
            //AddListeners();
        }
    
        /// <summary>
        /// Shows the banner ad. If not loaded, requests and shows it.
        /// Starts a timer to reload the ad after a specified duration.
        /// </summary>
        public override void ShowAd()
        {
            if (AdsItem != null)
            {
                if (!_isShowing)
                {
                    AdsItem.LoadAd(new AdRequest());
                    _isShowing = true;
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    StartReloadTimer(_cancellationTokenSource.Token).Forget();
                }
            }
            else
            {
                _isShowing = false;
                //_cancellationTokenSource?.Cancel();
                RequestAd();
                ShowAd();
            }
        }
    
        /// <summary>
        /// Starts a timer to reload the banner ad after the specified duration.
        /// </summary>
        /// <param name="token">Cancellation token to stop the timer.</param>
        private async UniTask StartReloadTimer(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromMinutes(AdsItemModel.Duration), cancellationToken: token);
    
            _isShowing = false;
            if (!token.IsCancellationRequested)
            {
                DestroyAd();
                ShowAd();
            }
        }
    
        /// <summary>
        /// Hides the banner ad if it is currently shown.
        /// </summary>
        public override void HideAd()
        {
            if (AdsItem is not null)
            {
                AdsItem.Hide();
            }
        }
    
        /// <summary>
        /// Destroys the banner ad and cancels any running reload timer.
        /// </summary>
        public override void DestroyAd()
        {
            if (AdsItem != null)
            {
                _isShowing = false;
                _cancellationTokenSource?.Cancel();
                //RemoveListeners();
                AdsItem.Destroy();
                AdsItem = null;
            }
        }
    
        /// <summary>
        /// Disposes resources used by the banner ad, including cancelling the reload timer.
        /// </summary>
        public override void Dispose()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}