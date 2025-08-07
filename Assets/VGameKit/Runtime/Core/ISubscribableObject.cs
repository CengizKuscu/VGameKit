using System;

namespace VGameKit.Runtime.Core
{
    /// <summary>
    /// Represents an object that supports initialization and subscription management,
    /// and can be disposed to release resources.
    /// </summary>
    public interface ISubscribableObject : IDisposable
    {
        /// <summary>
        /// Initializes the object.
        /// </summary>
        void Init();
        
        /// <summary>
        /// Sets up or manages subscriptions for the object.
        /// </summary>
        void Subscriptions();
    }
}