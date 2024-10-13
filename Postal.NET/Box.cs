using System;
using System.Threading;
using System.Threading.Tasks;

namespace PostalNET
{
    public sealed class Box : IBox, IChannelTopicMatcherProvider
    {
        public ISubscriberStore SubscriberStore { get; set; } = new BasicSubscriberStore();

        public IChannelTopicMatcher Matcher
        {
            get
            {
                return this.SubscriberStore is IChannelTopicMatcherProvider ? (this.SubscriberStore as IChannelTopicMatcherProvider).Matcher : null;
            }
        }

        public IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            Validate(channel, topic);
            Validate(subscriber);

            condition ??= (env) => true;

            return this.SubscriberStore.Subscribe(channel, topic, subscriber, condition);
        }

        public async Task PublishAsync(string channel, string topic, object data, CancellationToken cancellationToken = default)
        {
            Validate(channel, topic);

            var env = this.SubscriberStore.CreateEnvelope(channel, topic, data);

            await this.SubscriberStore.PublishAsync(env, cancellationToken);
        }

        private static void Validate(string channel, string topic)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(channel, nameof(channel));
            ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));          
        }

        private static void Validate(Action<Envelope> subscriber)
        {
            ArgumentNullException.ThrowIfNull(subscriber, nameof(subscriber));
        }
    }
}