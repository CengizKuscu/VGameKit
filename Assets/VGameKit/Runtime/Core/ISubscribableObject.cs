using System;

namespace VGameKit.Runtime.Core
{
    public interface ISubscribableObject : IDisposable
    {
        void Init();
        
        void Subscriptions();
    }
}