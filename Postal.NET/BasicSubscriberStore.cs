using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PostalNET
{
    public class BasicSubscriberStore : ISubscriberStore, IChannelTopicMatcherProvider
    {
        class SubscriberId
        {
            private readonly Guid _id;
            private readonly IChannelTopicMatcher _matcher;
            private readonly Func<Envelope, bool> _condition;
            private readonly string _channel;
            private readonly string _topic;

            public SubscriberId(string channel, string topic, Func<Envelope, bool> condition, IChannelTopicMatcher matcher)
            {
                this._id = Guid.NewGuid();
                this._channel = channel;
                this._topic = topic;
                this._matcher = matcher;
                this._condition = condition;
            }

            public bool MatchesChannelAndTopic(string channel, string topic)
            {
                return (this._matcher.Matches(this._channel, channel) == true)
                       && (this._matcher.Matches(this._topic, topic) == true);
            }

            public override bool Equals(object obj)
            {
                if (obj is not SubscriberId other)
                {
                    return false;
                }

                return other._id == this._id;
            }

            public override int GetHashCode()
            {
                return this._id.GetHashCode();
            }

            public bool PassesCondition(Envelope envelope)
            {
                return this._condition(envelope);
            }
        }

        class DisposableSubscription : IDisposable
        {
            private readonly SubscriberId _id;
            private readonly IDictionary<SubscriberId, GCHandle> _subscribers;

            public DisposableSubscription(SubscriberId id, IDictionary<SubscriberId, GCHandle> subscribers)
            {
                this._id = id;
                this._subscribers = subscribers;
            }

            public void Dispose()
            {
                var handle = this._subscribers[this._id];
                handle.Free();
                this._subscribers.Remove(this._id);
            }
        }

        private readonly ConcurrentDictionary<SubscriberId, GCHandle> _subscribers = new();

        public IChannelTopicMatcher Matcher { get; set; } = WildcardChannelTopicMatcher.Instance;
        public IPublisher Publisher { get; set; } = AsyncPublisher.Instance;

        public virtual IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            var id = this.CreateId(channel, topic, condition) as SubscriberId;
            this._subscribers[id] = GCHandle.Alloc(subscriber, GCHandleType.Weak);
            return new DisposableSubscription(id, this._subscribers);
        }

        public virtual async Task PublishAsync(Envelope envelope, CancellationToken cancellationToken = default)
        {
            await this.Publisher.PublishAsync(this.GetSubscribers(envelope), envelope, cancellationToken);
        }

        public virtual Envelope CreateEnvelope(string channel, string topic, object data)
        {
            return new Envelope(channel, topic, data);
        }

        protected virtual object CreateId(string channel, string topic, Func<Envelope, bool> condition)
        {
            var id = new SubscriberId(channel, topic, condition, this.Matcher);
            return id;
        }

        protected virtual IEnumerable<Action<Envelope>> GetSubscribers(Envelope envelope)
        {
            return this._subscribers
                .AsParallel()
                .Where(subscriber =>
                    (subscriber.Value.IsAllocated == true) &&
                    (this.MatchesChannelAndTopic(subscriber.Key, envelope.Channel, envelope.Topic) == true) &&
                    (this.PassesCondition(subscriber.Key, envelope) == true))
                .Select(subscriber => (Action<Envelope>)subscriber.Value.Target);
        }

        protected virtual bool MatchesChannelAndTopic(object id, string channel, string topic)
        {
            return (id as SubscriberId).MatchesChannelAndTopic(channel, topic);
        }

        protected virtual bool PassesCondition(object id, Envelope envelope)
        {
            return (id as SubscriberId).PassesCondition(envelope);
        }
    }
}