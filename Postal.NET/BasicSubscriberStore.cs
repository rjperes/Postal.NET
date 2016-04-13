using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PostalNET
{
    public class BasicSubscriberStore : ISubscriberStore
    {
        class SubscriberId
        {
            private readonly Guid id;
            private readonly IChannelTopicMatcher matcher;
            private readonly Func<Envelope, bool> condition;
            private readonly string channel;
            private readonly string topic;

            public SubscriberId(string channel, string topic, Func<Envelope, bool> condition, IChannelTopicMatcher matcher)
            {
                this.id = Guid.NewGuid();
                this.channel = channel;
                this.topic = topic;
                this.matcher = matcher;
                this.condition = condition;
            }

            public bool MatchesChannelAndTopic(string channel, string topic)
            {
                return (this.matcher.Matches(this.channel, channel) == true)
                       && (this.matcher.Matches(this.topic, topic) == true);
            }

            public override bool Equals(object obj)
            {
                var other = obj as SubscriberId;

                if (other == null)
                {
                    return false;
                }

                return other.id == this.id;
            }

            public override int GetHashCode()
            {
                return this.id.GetHashCode();
            }

            public bool PassesCondition(Envelope env)
            {
                return this.condition(env);
            }
        }

        class DisposableSubscription : IDisposable
        {
            private readonly SubscriberId id;
            private readonly IDictionary<SubscriberId, GCHandle> subscribers;

            public DisposableSubscription(SubscriberId id, IDictionary<SubscriberId, GCHandle> subscribers)
            {
                this.id = id;
                this.subscribers = subscribers;
            }

            public void Dispose()
            {
                var handle = this.subscribers[this.id];
                handle.Free();
                this.subscribers.Remove(this.id);
            }
        }

        private readonly ConcurrentDictionary<SubscriberId, GCHandle> subscribers = new ConcurrentDictionary<SubscriberId, GCHandle>();

        public BasicSubscriberStore()
        {
            this.Matcher = WildcardChannelTopicMatcher.Instance;
        }

        public IChannelTopicMatcher Matcher { get; set; }

        protected virtual object CreateId(string channel, string topic, Func<Envelope, bool> condition)
        {
            var id = new SubscriberId(channel, topic, condition, this.Matcher);
            return id;
        }

        public virtual IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            var id = this.CreateId(channel, topic, condition) as SubscriberId;
            this.subscribers[id] = GCHandle.Alloc(subscriber, GCHandleType.Weak);
            return new DisposableSubscription(id, this.subscribers);
        }

        protected virtual IEnumerable<Action<Envelope>> GetSubscribers(Envelope env)
        {
            return this.subscribers
                .AsParallel()
                .Where(subscriber =>
                    (subscriber.Value.IsAllocated == true) &&
                    (this.MatchesChannelAndTopic(subscriber.Key, env.Channel, env.Topic) == true) &&
                    (this.PassesCondition(subscriber.Key, env) == true))
                .Select(subscriber => (Action<Envelope>)subscriber.Value.Target);
        }

        public virtual async Task PublishAsync(Envelope env)
        {
            foreach (var subscriber in this.GetSubscribers(env).AsParallel())
            {
                await Task.Run(() => subscriber(env));
            }
        }

        public virtual void Publish(Envelope env)
        {
            this.PublishAsync(env).GetAwaiter().GetResult();
        }

        protected virtual bool MatchesChannelAndTopic(object id, string channel, string topic)
        {
            return (id as SubscriberId).MatchesChannelAndTopic(channel, topic);
        }

        protected virtual bool PassesCondition(object id, Envelope env)
        {
            return (id as SubscriberId).PassesCondition(env);
        }
    }
}