using MessagePipe;
using UnityEngine;
using VContainer;
using VGameKit.Runtime.Log;

namespace VGameKit.Runtime.Core
{
    /// <summary>
    /// Abstract main lifetime scope for the application.
    /// Handles frame rate settings, logging configuration, and MessagePipe registration.
    /// </summary>
    public abstract class AbsMainLifetimeScope : AbsBaseLifetimeScope
    {
        /// <summary>
        /// Target frame rate for the application.
        /// </summary>
        [SerializeField] protected int _targetFrameRate = 60;
    
        /// <summary>
        /// Options for MessagePipe.
        /// </summary>
        protected MessagePipeOptions _messagePipeOpts;
    
        /// <summary>
        /// Configures the container builder with logging, frame rate, and MessagePipe options.
        /// </summary>
        /// <param name="builder">The container builder to configure.</param>
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
    
    #if UNITY_EDITOR || GAMEKIT_LOG || GOOGLEADS_TESTDEVICE
            Debug.unityLogger.logEnabled = true;
            GKLog.ReportLogLevel();
    #else
            Debug.unityLogger.logEnabled = true;
    #endif
    
            Application.targetFrameRate = _targetFrameRate;
    
            _messagePipeOpts = builder.RegisterMessagePipe();
        }
    }
}