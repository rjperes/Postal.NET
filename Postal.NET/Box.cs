using System;
using System.Threading.Tasks;

namespace Postal.NET
{
    public sealed class Box : IBox
    {
        public ISubscriberStore SubscriberStore { get; set; } = new BasicSubscriberStore();

        public void Publish(string channel, string topic, object data)
        {
            this
                .PublishAsync(channel, topic, data)
                .GetAwaiter()
                .GetResult();
        }

        public IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            this.Validate(channel, topic);
            this.Validate(subscriber);

            if (condition == null)
            {
                condition = (env) => true;
            }

            return this.SubscriberStore.Subscribe(channel, topic, subscriber, condition);
        }

        public async Task PublishAsync(string channel, string topic, object data)
        {
            this.Validate(channel, topic);

            var env = new Envelope(channel, topic, data);

            await this.SubscriberStore.PublishAsync(env);
        }

        private void Validate(string channel, string topic)
        {
            if (string.IsNullOrWhiteSpace(channel) == true)
            {
                throw new ArgumentNullException("channel");
            }

            if (string.IsNullOrWhiteSpace(topic) == true)
            {
                throw new ArgumentNullException("topic");
            }

            if (channel.Contains(",") == true)
            {
                throw new ArgumentException("Channel names cannot contain commas", "channel");
            }

            if (topic.Contains(",") == true)
            {
                throw new ArgumentException("Topic names cannot contain commas", "topic");
            }
        }

        private void Validate(Action<Envelope> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }
        }
    }
}