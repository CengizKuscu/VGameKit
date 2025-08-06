using System;
using System.Diagnostics;
using System.Text;
using VGameKit.Runtime.Config;

namespace VGameKit.Runtime.Log
{
    /// <summary>
    /// Provides logging functionality for the VGameKit runtime.
    /// Supports multiple log levels and color-coded output in Unity.
    /// </summary>
    public static class GKLog
    {
        /// <summary>
        /// Current log state, determines which log levels are enabled.
        /// </summary>
        private static LogState _logState = LogState.None;
    
        /// <summary>
        /// Static constructor initializes the log state from configuration and reports the log level.
        /// </summary>
        static GKLog()
        {
            if (GKConfig.Instance is { } config)
            {
                _logState = config.LogState;
            }
            else
            {
                _logState = LogState.None;
            }
        }
    
        /// <summary>
        /// Logs the current log state at the Core level to indicate logger readiness.
        /// </summary>
        public static void ReportLogLevel()
        {
            Log(LogState.Core, $"GKLog is READY with LogState: {Enum.GetName(typeof(LogState), _logState)}");
        }
    
        /// <summary>
        /// Logs a message with the specified log state if enabled.
        /// Output is color-coded based on log state for Unity console.
        /// Only compiled if GAMEKIT_LOG is defined.
        /// </summary>
        /// <param name="logState">The log state/category for the message.</param>
        /// <param name="message">The message to log.</param>
        [Conditional("GAMEKIT_LOG")]
        public static void Log(LogState logState, object message)
        {
            if (!_logState.HasFlag(logState)) return;
    
            string sb = "";
    
            switch (logState)
            {
                case LogState.Core:
                case LogState.Development:
                case LogState.Info:
                    sb = $"[GKLog.{logState}] : {message}";
                    UnityEngine.Debug.Log(sb);
                    break;
                case LogState.Error:
                case LogState.Fatal:
                    sb = $"<color=red>[GKLog.{logState}] : {message}</color>";
                    UnityEngine.Debug.Log(sb);
                    break;
                case LogState.Pause:
                    sb = $"<color=blue>[GKLog.{logState}] : {message}</color>";
                    UnityEngine.Debug.Log(sb);
                    break;
                case LogState.Game:
                    sb = $"<color=green>[GKLog.{logState}] : {message}</color>";
                    UnityEngine.Debug.Log(sb);
                    break;
                case LogState.Warning:
                case LogState.ProcessFlow:
                    sb = $"<color=yellow>[GKLog.{logState}] : {message}</color>";
                    UnityEngine.Debug.Log(sb);
                    break;
                case LogState.Analytics:
                case LogState.Booster:
                case LogState.Timer:
                case LogState.IAP:
                case LogState.Ads:
                    sb = $"<color=magenta>[GKLog.{logState}] : {message}</color>";
                    UnityEngine.Debug.Log(sb);
                    break;
            }
        }
    }
}