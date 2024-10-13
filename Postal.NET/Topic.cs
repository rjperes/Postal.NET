using System;
using System.Threading;
using System.Threading.Tasks;

namespace PostalNET
{
    internal sealed class Topic : ITopic
    {
        private readonly IBox _box;
        private readonly string _channel;
        private readonly string _topic;

        public Topic(IBox box, string channel, string topic)
        {
            this._box = box;
            this._channel = channel;
            this._topic = topic;
        }

        public IDisposable SubscribeWhen(Action<Envelope> subscriber, Func<Envelope, bool> condition)
        {
            return this._box.Subscribe(this._channel, this._topic, subscriber, condition);
        }

        public IDisposable Subscribe(Action<Envelope> subscriber)
        {
            return this.SubscribeWhen(subscriber, null);
        }

        public async Task PublishAsync(object data, CancellationToken cancellationToken = default)
        {
            await this._box.PublishAsync(this._channel, this._topic, data, cancellationToken);
        }

        public override string ToString()
        {
            return this._topic;
        }
    }
}