using System;
using System.Threading.Tasks;

namespace Postal.NET
{
    public interface ISubscriberStore
    {
        IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition);
        Task PublishAsync(Envelope env);
        void Publish(Envelope env);
    }
}
