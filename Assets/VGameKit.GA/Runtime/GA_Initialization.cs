using Cysharp.Threading.Tasks;
using GameAnalyticsSDK;
using UnityEngine;

namespace VGameKit.GA.Runtime
{
    /// <summary>
    /// Initializes GameAnalytics and handles ATT (App Tracking Transparency) requests on iOS.
    /// </summary>
    public class GA_Initialization : MonoBehaviour, IGameAnalyticsATTListener
    {
        /// <summary>
        /// Indicates whether GameAnalytics has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Initializes GameAnalytics and requests ATT authorization on iOS.
        /// </summary>
        public async UniTask Initialize()
        {
            print("[VGameKit] GA_Initialization.Initialize Beginning");
            GameAnalytics.onInitialize += OnGameAnalyticsInitialized;
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                GameAnalytics.RequestTrackingAuthorization(this);
            }
            else
            {
                GameAnalytics.Initialize();
            }

            if (IsInitialized == false)
                await UniTask.WaitUntil(() => IsInitialized);
            
        }

        /// <summary>
        /// Callback for when GameAnalytics initialization is complete.
        /// This method is called when the GameAnalytics SDK has finished initializing.
        /// It checks if the initialization was successful and updates the IsInitialized property accordingly.
        /// If the initialization fails, it logs an error message.
        /// The event listener is removed after handling the initialization to prevent memory leaks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameAnalyticsInitialized(object sender, bool e)
        {
            if (e)
            {
                print("[VGameKit] OnGameAnalyticsInitialized");
            }
            else
            {
                print("[VGameKit] OnGameAnalyticsInitialized failed");
            }

            IsInitialized = GameAnalytics.Initialized;
            GameAnalytics.onInitialize -= OnGameAnalyticsInitialized;
            
        }

        /// <summary>
        /// Handles the ATT authorization status when it is not determined. 
        /// </summary>
        public void GameAnalyticsATTListenerNotDetermined()
        {
            GameAnalytics.Initialize();
            print("[VGameKit] GameAnalyticsATTListenerNotDetermined");
        }

        /// <summary>
        /// Handles the ATT authorization status when it is restricted.
        /// </summary>
        public void GameAnalyticsATTListenerRestricted()
        {
            GameAnalytics.Initialize();
            print("[VGameKit] GameAnalyticsATTListenerRestricted");
        }

        /// <summary>
        /// Handles the ATT authorization status when it is denied.
        /// </summary>
        public void GameAnalyticsATTListenerDenied()
        {
            GameAnalytics.Initialize();
            print("[VGameKit] GameAnalyticsATTListenerDenied");
        }

        /// <summary>
        /// Handles the ATT authorization status when it is authorized.
        /// </summary>
        public void GameAnalyticsATTListenerAuthorized()
        {
            GameAnalytics.Initialize();
            print("[VGameKit] GameAnalyticsATTListenerAuthorized");
        }
    }
}