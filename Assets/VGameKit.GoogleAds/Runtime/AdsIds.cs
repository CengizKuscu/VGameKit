using System;
using UnityEngine;

namespace VGameKit.GoogleAds.Runtime
{
    /// <summary>
    /// Serializable struct to hold AdMob ad unit IDs for Android and iOS platforms,
    /// including both production and test IDs. Provides helper methods to retrieve
    /// the correct ad unit ID based on platform and test mode.
    /// </summary>
    [Serializable]
    public struct AdsIds
    {
        /// <summary>
        /// Android Banner Ad Unit ID (production).
        /// </summary>
        [Header("Android")] public string androidBannerId;

        /// <summary>
        /// Android Interstitial Ad Unit ID (production).
        /// </summary>
        public string androidInterstitialId;

        /// <summary>
        /// Android Rewarded Ad Unit ID (production).
        /// </summary>
        public string androidRewardedId;

        [Space(10f)]
        /// <summary>
        /// iOS Banner Ad Unit ID (production).
        /// </summary>
        [Header("iOS")]
        public string iosBannerId;

        /// <summary>
        /// iOS Interstitial Ad Unit ID (production).
        /// </summary>
        public string iosInterstitialId;

        /// <summary>
        /// iOS Rewarded Ad Unit ID (production).
        /// </summary>
        public string iosRewardedId;

        /// <summary>
        /// Android test Banner Ad Unit ID (provided by Google).
        /// </summary>
        public const string drdTestBannerId = "ca-app-pub-3940256099942544/6300978111";

        /// <summary>
        /// Android test Interstitial Ad Unit ID (provided by Google).
        /// </summary>
        public const string drdTestInterstitialId = "ca-app-pub-3940256099942544/1033173712";

        /// <summary>
        /// Android test Rewarded Ad Unit ID (provided by Google).
        /// </summary>
        public const string drdTestRewardedId = "ca-app-pub-3940256099942544/5224354917";

        /// <summary>
        /// iOS test Banner Ad Unit ID (provided by Google).
        /// </summary>
        public const string iosTestBannerId = "ca-app-pub-3940256099942544/2934735716";

        /// <summary>
        /// iOS test Interstitial Ad Unit ID (provided by Google).
        /// </summary>
        public const string iosTestInterstitialId = "ca-app-pub-3940256099942544/4411468910";

        /// <summary>
        /// iOS test Rewarded Ad Unit ID (provided by Google).
        /// </summary>
        public const string iosTestRewardedId = "ca-app-pub-3940256099942544/1712485313";

        /// <summary>
        /// Returns the appropriate Banner Ad Unit ID for the current platform.
        /// </summary>
        /// <param name="useTestAds">If true, returns the test ad unit ID; otherwise, returns the production ID.</param>
        /// <returns>Banner Ad Unit ID string for the current platform.</returns>
        public string GetBannerId(bool useTestAds)
        {
#if UNITY_ANDROID
            return useTestAds ? drdTestBannerId : drdBannerId;
#elif UNITY_IPHONE
            return useTestAds ? iosTestBannerId : iosBannerId;
#else
            return "";
#endif
        }

        /// <summary>
        /// Returns the appropriate Interstitial Ad Unit ID for the current platform.
        /// </summary>
        /// <param name="useTestAds">If true, returns the test ad unit ID; otherwise, returns the production ID.</param>
        /// <returns>Interstitial Ad Unit ID string for the current platform.</returns>
        public string GetInterstitialId(bool useTestAds)
        {
#if UNITY_ANDROID
            return useTestAds ? drdTestInterstitialId : drdInterstitialId;
#elif UNITY_IPHONE
            return useTestAds ? iosTestInterstitialId : iosInterstitialId;
#else
            return "";
#endif
        }

        /// <summary>
        /// Returns the appropriate Rewarded Ad Unit ID for the current platform.
        /// </summary>
        /// <param name="useTestAds">If true, returns the test ad unit ID; otherwise, returns the production ID.</param>
        /// <returns>Rewarded Ad Unit ID string for the current platform.</returns>
        public string GetRewardedId(bool useTestAds)
        {
#if UNITY_ANDROID
            return useTestAds ? drdTestRewardedId : drdRewardedId;
#elif UNITY_IPHONE
            return useTestAds ? iosTestRewardedId : iosRewardedId;
#else
            return "";
#endif
        }
    }
}