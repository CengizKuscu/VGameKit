using System.Threading;
using Cysharp.Threading.Tasks;

namespace VGameKit.Runtime.ProcessFlows
{
    /// <summary>
        /// Represents an asynchronous flow task that can be executed.
        /// </summary>
        /// <typeparam name="T">The type implementing <see cref="IFlowAsyncTask{T}"/>.</typeparam>
        public interface IFlowAsyncTask<T> where T : IFlowAsyncTask<T>
        {
            /// <summary>
            /// Executes the asynchronous task.
            /// </summary>
            /// <param name="ctx">A cancellation token to observe while waiting for the task to complete.</param>
            /// <returns>A <see cref="UniTask{T}"/> representing the asynchronous operation.</returns>
            UniTask<T> Execute(CancellationToken ctx = default);
        }
}