namespace VGameKit.Runtime.Utilities
{
    /// <summary>
    /// Provides support detection for haptic (vibration) feedback on the current device.
    /// </summary>
    public static class HapticSupport
    {
        /// <summary>
        /// Cached value indicating whether haptic feedback is supported.
        /// </summary>
        private static bool? _supportHaptic;
    
        /// <summary>
        /// Gets a value indicating whether haptic feedback is supported on the current device.
        /// Returns <c>true</c> in the Unity Editor, otherwise checks <c>UnityEngine.SystemInfo.supportsVibration</c>.
        /// </summary>
        public static bool SupportHaptic
        {
            get
            {
                if (!_supportHaptic.HasValue)
                {
    #if !UNITY_EDITOR
                    _supportHaptic = UnityEngine.SystemInfo.supportsVibration;
    #else
                    _supportHaptic = true;
    #endif
                }
    
                return _supportHaptic.Value;
            }
        }
    }
}