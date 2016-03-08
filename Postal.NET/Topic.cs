using System;
using System.Threading.Tasks;

namespace Postal.NET
{
    internal sealed class Topic : ITopic
    {
        private readonly IBox box;
        private readonly string channel;
        private readonly string topic;

        public Topic(IBox box, string channel, string topic)
        {
            this.box = box;
            this.channel = channel;
            this.topic = topic;
        }

        public IDisposable SubscribeWhen(Action<Envelope> subscriber, Func<Envelope, bool> condition)
        {
            return this.box.Subscribe(this.channel, this.topic, subscriber, condition);
        }

        public IDisposable Subscribe(Action<Envelope> subscriber)
        {
            return this.SubscribeWhen(subscriber, null);
        }

        public void Publish(object data)
        {
            this.box.Publish(this.channel, this.topic, data);
        }

        public async Task PublishAsync(object data)
        {
            await this.box.PublishAsync(this.channel, this.topic, data);
        }

        public override string ToString()
        {
            return this.topic;
        }
    }
}