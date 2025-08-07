namespace VGameKit.Runtime.ProcessFlows
{
    /// <summary>
    /// Represents a flow task that can be executed and returns an instance of itself.
    /// </summary>
    /// <typeparam name="T">The type of the flow task implementing this interface.</typeparam>
    public interface IFlowTask<T> where T : IFlowTask<T>
    {
        /// <summary>
        /// Executes the flow task and returns the result as an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/> representing the result of the execution.</returns>
        T Execute();
    }
}