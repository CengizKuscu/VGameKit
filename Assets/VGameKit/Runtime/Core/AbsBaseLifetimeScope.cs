using VContainer;
using VContainer.Unity;
using VGameKit.Runtime.Log;

namespace VGameKit.Runtime.Core
{
    /// <summary>
    /// Abstract base class for lifetime scopes in the VGameKit runtime.
    /// Manages the readiness state of the scope.
    /// </summary>
    public abstract class AbsBaseLifetimeScope : LifetimeScope 
    {
        /// <summary>
        /// Indicates whether the lifetime scope is ready.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Configures the container builder and sets up the readiness callback.
        /// </summary>
        /// <param name="builder">The container builder to configure.</param>
        protected override void Configure(IContainerBuilder builder)
        {
            IsReady = false;
            builder.RegisterBuildCallback(_ =>
            {
                IsReady = true;
                GKLog.Log(LogState.Core, $"{this.GetType().Name} is ready.");
            });
        }
    }
}