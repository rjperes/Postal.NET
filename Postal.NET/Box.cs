using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Postal.NET
{
    public sealed class Box : IBox
    {
        class SubscriberId
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
                this.subscribers.Remove(this.id);
            }
        }

        private readonly ConcurrentDictionary<SubscriberId, GCHandle> subscribers = new ConcurrentDictionary<SubscriberId, GCHandle>();

        public void Publish(string channel, string topic, object data)
        {
            this.Validate(channel, topic);
            this.PublishAsync(channel, topic, data)
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

            var id = new SubscriberId(channel, topic, condition);

            this.subscribers[id] = GCHandle.Alloc(subscriber);

            return new DisposableSubscription(id, this.subscribers);
        }

        public async Task PublishAsync(string channel, string topic, object data)
        {
            this.Validate(channel, topic);

            var env = new Envelope(channel, topic, data);

            foreach (var subscriber in this.GetSubscribers(channel, topic, env).AsParallel())
            {
                await Task.Run(() => subscriber(env));
            }
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
        }

        private void Validate(Action<Envelope> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }
        }

        private void Validate(Func<Envelope, bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException("condition");
            }
        }

        private bool MatchesChannelAndTopic(SubscriberId id, string channel, string topic)
        {
            return id.MatchesChannelAndTopic(channel, topic);
        }

        private bool PassesCondition(SubscriberId id, Envelope env)
        {
            return id.PassesCondition(env);
        }

        private IEnumerable<Action<Envelope>> GetSubscribers(string channel, string topic, Envelope env)
        {
            return this.subscribers
                .Where(subscriber =>
                    (subscriber.Value.IsAllocated == true) &&
                    (this.MatchesChannelAndTopic(subscriber.Key, channel, topic) == true) &&
                    (this.PassesCondition(subscriber.Key, env) == true))
                .Select(subscriber => (Action<Envelope>) subscriber.Value.Target);
        }
    }
}