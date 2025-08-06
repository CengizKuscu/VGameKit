using System.IO;
using System.Linq;
using UnityEngine;
using VGameKit.Runtime.Log;

namespace VGameKit.Runtime.Config
{
    /// <summary>
    /// GKConfig is a singleton ScriptableObject for managing global configuration settings,
    /// including logging state, for the VGameKit runtime.
    /// It supports both editor and runtime asset loading, and ensures only one instance is used.
    /// </summary>
    public class GKConfig : ScriptableObject
    {
        /// <summary>
        /// The current logging state for the application.
        /// </summary>
        [field: SerializeField]
        public LogState LogState { get; private set; } = LogState.None;

        private static GKConfig _instance;
        //public static GKConfig Instance { get; private set; }

        /// <summary>
        /// Singleton instance accessor for GKConfig.
        /// Loads the asset from preloaded assets in the editor, or from Resources at runtime.
        /// </summary>
        public static GKConfig Instance
        {
            get
            {
                // Eğer daha önce atanmışsa
                if (_instance != null)
                    return _instance;

#if UNITY_EDITOR
                // Editor modunda: preloaded assets'te arıyoruz
                var preloaded =
                    UnityEditor.PlayerSettings.GetPreloadedAssets().FirstOrDefault(x => x is GKConfig) as GKConfig;
                if (preloaded != null)
                {
                    _instance = preloaded;
                    return _instance;
                }
#endif
                // Runtime veya fallback: Resources üzerinden deniyoruz
                var resourceLoaded = Resources.Load<GKConfig>("GKConfig");
                if (resourceLoaded != null)
                {
                    _instance = resourceLoaded;
                    return _instance;
                }

                Debug.LogWarning(
                    "[GKConfig] Instance not found! Please create a GKConfig asset via the editor or add it to Resources folder as GKConfig.asset.");
                return null;
            }
            private set { _instance = value; }
        }


#if UNITY_EDITOR
        /// <summary>
        /// Creates a GKConfig asset in the Resources folder and adds it to preloaded assets.
        /// Accessible via the Unity editor menu.
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/VGameKit/Config/GKConfig")]
        public static void CreateSettingsAsset()
        {
            var assetName = "GKConfig";
            var assetExtension = "asset";

            var path = "Assets/Resources/" + assetName + "." + assetExtension;

            if (string.IsNullOrEmpty(path))
                return;

            // Eğer Resources klasörü yoksa oluştur
            var resourcesPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }

            var tempConfig = ScriptableObject.CreateInstance<GKConfig>();
            UnityEditor.AssetDatabase.CreateAsset(tempConfig, path);

            var loadedAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<GKConfig>(path);
            if (loadedAsset == null)
            {
                Debug.LogError("[GKConfig] Asset could not be created!");
                return;
            }

            var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.RemoveAll(x => x is GKConfig);
            preloadedAssets.Add(loadedAsset);
            UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log(
                "[GKConfig] GKConfig asset created and added to Preloaded Assets. You can set it up in the inspector.");
            UnityEditor.EditorGUIUtility.PingObject(loadedAsset);
        }

        /// <summary>
        /// Ensures the GKConfig instance is loaded from preloaded assets before the first scene loads.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadInstanceFromPreloadAssetsOnLoad()
        {
            var inst = Instance;
            if (inst == null)
            {
                Debug.LogWarning(
                    "[GKConfig] GKConfig asset not found in Resources or as preloaded asset. Please create and set up the asset.");
            }
        }
#endif

        /// <summary>
        /// Ensures the singleton instance is set when the asset is enabled.
        /// Warns if multiple instances are loaded.
        /// </summary>
        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[GKConfig] Multiple GKConfig assets loaded! Only the first one found will be used.");
            }
        }
    }
}