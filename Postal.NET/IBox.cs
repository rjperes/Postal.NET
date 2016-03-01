using System;
using System.Threading.Tasks;

namespace Postal.NET
{
    public interface IBox
    {
        IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null);

        void Publish(string channel, string topic, object data);
        Task PublishAsync(string channel, string topic, object data);
    }
}
