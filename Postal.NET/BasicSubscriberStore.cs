using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Postal.NET
{
    public class BasicSubscriberStore : ISubscriberStore
    {
        protected class SubscriberId
        {
            private readonly Guid id;
            private readonly Regex channelRegex;
            private readonly Regex topicRegex;
            private readonly Func<Envelope, bool> condition;

            public SubscriberId(string channel, string topic, Func<Envelope, bool> condition)
            {
                this.id = Guid.NewGuid();
                this.channelRegex = new Regex("^" + this.Normalize(channel) + "$");
                this.topicRegex = new Regex("^" + this.Normalize(topic) + "$");
                this.condition = condition;
            }

            private string Normalize(string str)
            {
                return str
                    .Replace(".", "\\.")
                    .Replace(Postal.All, "." + Postal.All);
            }

            public bool MatchesChannelAndTopic(string channel, string topic)
            {
                return this.channelRegex.IsMatch(channel) == true
                       && this.topicRegex.IsMatch(topic);
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

        public virtual IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            var id = new SubscriberId(channel, topic, condition);
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

        protected virtual bool MatchesChannelAndTopic(SubscriberId id, string channel, string topic)
        {
            return id.MatchesChannelAndTopic(channel, topic);
        }

        protected virtual bool PassesCondition(SubscriberId id, Envelope env)
        {
            return id.PassesCondition(env);
        }
    }
}