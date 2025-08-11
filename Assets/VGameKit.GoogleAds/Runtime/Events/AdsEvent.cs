namespace VGameKit.GoogleAds.Runtime.Events
{
    /// <summary>
    /// Represents an advertisement event, including its type, source, and related properties.
    /// </summary>
    public class AdsEvent
    {
        /// <summary>
        /// Gets the type of the advertisement.
        /// </summary>
        public AdsType AdsType { get; private set; }
    
        /// <summary>
        /// Gets the source or context from which the ad event originated.
        /// </summary>
        public string From { get; private set; }
    
        /// <summary>
        /// Gets or sets the identifier for the ad. Defaults to -1 if not set.
        /// </summary>
        public int AdId { get; set; } = -1;
    
        /// <summary>
        /// Gets or sets a value indicating whether a reward was earned from the ad.
        /// </summary>
        public bool IsEarnedReward { get; set; } = false;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="AdsEvent"/> class.
        /// </summary>
        /// <param name="adsType">The type of the advertisement.</param>
        /// <param name="from">The source or context of the ad event.</param>
        /// <param name="adId">The identifier for the ad. Defaults to -1.</param>
        public AdsEvent(AdsType adsType, string from = "", int adId = -1)
        {
            AdsType = adsType;
            From = from;
            AdId = adId;
        }
    }
}