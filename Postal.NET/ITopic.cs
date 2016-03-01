using System;
using System.Threading.Tasks;

namespace Postal.NET
{
    public interface ITopic
    {
        IDisposable SubscribeWhen(Action<Envelope> subscriber, Func<Envelope, bool> condition);
        IDisposable Subscribe(Action<Envelope> subscriber);

        void Publish(object data);
        Task PublishAsync(object data);
    }
}