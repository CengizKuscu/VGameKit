using System.Runtime.CompilerServices;
using VContainer;

namespace VGameKit.Runtime.ProcessFlows
{
    /// <summary>
    /// Extension methods for registering process flows with the container.
    /// </summary>
    public static class ProcessFlowsExtensions
    {
        /// <summary>
        /// Registers a process flow factory with the container builder.
        /// </summary>
        /// <typeparam name="TArgs">Type of process flow arguments.</typeparam>
        /// <typeparam name="TProcessFlow">Type of process flow.</typeparam>
        /// <param name="builder">The container builder.</param>
        /// <param name="lifeTime">The lifetime of the registration.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterProcessFlow<TArgs, TProcessFlow>(this IContainerBuilder builder, Lifetime lifeTime)
            where TArgs : IProcessFlowArgs
            where TProcessFlow : IProcessFlow<TArgs>, new()
        {
            builder.RegisterFactory<TArgs, TProcessFlow>(container =>
            {
                var provider = container.Resolve<ProcessFlowProvider>();
                return args => provider.CreateProcessFlow<TArgs, TProcessFlow>(args);
            }, lifeTime);
        }
    }
}