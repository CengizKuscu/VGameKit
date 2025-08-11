using System;
using VGameKit.GoogleAds.Runtime.Events;

namespace VGameKit.GoogleAds.Runtime
{
    /// <summary>
    /// Abstract base class for ad views, providing common functionality for ad operations.
    /// </summary>
    /// <typeparam name="T">The ad item type.</typeparam>
    /// <typeparam name="TModel">The ad item model type, must inherit from AdsItemModel.</typeparam>
    public abstract class AdsBaseView<T, TModel> : IDisposable where T : class where TModel : AdsItemModel
    {
        /// <summary>
        /// Event triggered in response to ad events.
        /// </summary>
        public Action<AdsEvent, AdsEventStatus> onResponseAdEvent;
    
        /// <summary>
        /// The ad item instance.
        /// </summary>
        public T AdsItem { get; protected set; }
    
        /// <summary>
        /// The ad item model instance.
        /// </summary>
        public TModel AdsItemModel { get; private set; }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="AdsBaseView{T, TModel}"/> class.
        /// </summary>
        /// <param name="adsItemModel">The ad item model.</param>
        public AdsBaseView(TModel adsItemModel)
        {
            AdsItemModel = adsItemModel;
        }
    
        /// <summary>
        /// Gets a value indicating whether the ad is loaded.
        /// </summary>
        public virtual bool IsLoaded { get; protected set; }
    
        /// <summary>
        /// Loads the ad.
        /// </summary>
        public virtual void LoadAd()
        {
        }
    
        /// <summary>
        /// Requests the ad.
        /// </summary>
        public virtual void RequestAd()
        {
        }
    
        /// <summary>
        /// Shows the ad.
        /// </summary>
        public virtual void ShowAd()
        {
        }
    
        /// <summary>
        /// Hides the ad.
        /// </summary>
        public virtual void HideAd()
        {
        }
    
        /// <summary>
        /// Destroys the ad and releases resources.
        /// </summary>
        public virtual void DestroyAd()
        {
        }
    
        /// <summary>
        /// Adds event listeners for ad events.
        /// </summary>
        public virtual void AddListeners()
        {
        }
    
        /// <summary>
        /// Removes event listeners for ad events.
        /// </summary>
        public virtual void RemoveListeners()
        {
        }
    
        /// <summary>
        /// Disposes resources used by the ad view.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
    
    /// <summary>
    /// Model representing ad item data.
    /// </summary>
    public class AdsItemModel
    {
        /// <summary>
        /// Gets or sets the ad type.
        /// </summary>
        public AdsType AdsType { get; set; }
    
        /// <summary>
        /// Gets or sets the ad identifier.
        /// </summary>
        public string AdsId { get; set; }
    
        /// <summary>
        /// Gets or sets the ad duration.
        /// </summary>
        public int Duration { get; set; }
    }
}