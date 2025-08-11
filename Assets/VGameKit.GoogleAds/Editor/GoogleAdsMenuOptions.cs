using UnityEditor;
using UnityEngine;
using VGameKit.GoogleAds.Runtime;

namespace VGameKit.GoogleAds.Editor
{
    /// <summary>
    /// Provides menu options for creating Google Ads related GameObjects in the Unity Editor.
    /// </summary>
    public static class GoogleAdsMenuOptions
    {
        /// <summary>
        /// Creates a new GameObject with a <see cref="GoogleMobileAdsController"/> component attached,
        /// and sets its local position to zero. Accessible from the Unity Editor menu: GameObject/VGameKit/Google Ads Controller.
        /// </summary>
        [MenuItem("GameObject/VGameKit/Google Ads Controller")]
        private static void CreateChefGoogleAdsController()
        {
            GameObject googleAdsManager = new GameObject("ChefGoogleAdsController1");
            var component = googleAdsManager.AddComponent<GoogleMobileAdsController>();
            googleAdsManager.transform.localPosition = Vector3.zero;
        }
    }
}