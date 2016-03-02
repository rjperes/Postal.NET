using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Postal.NET
{
    public sealed class Box : IBox
    {
        class SubscriberId
        {
            private readonly Guid id;
            private readonly string channel;
            private readonly string topic;
            private readonly int hash;
            private readonly Func<Envelope, bool> condition;

            public SubscriberId(string channel, string topic, Func<Envelope, bool> condition)
            {
                this.id = Guid.NewGuid();
                this.channel = channel;
                this.topic = topic;
                this.condition = condition;

                unchecked
                {
                    this.hash = 13;
                    this.hash = (this.hash * 17) ^ this.id.GetHashCode();
                    this.hash = (this.hash * 17) ^ this.channel.GetHashCode();
                    this.hash = (this.hash * 17) ^ this.topic.GetHashCode();
                }
            }

            private string Normalize(string str)
            {
                return str
                    .Replace(".", "\\.")
                    .Replace("*", ".*");
            }

            public bool MatchesChannelAndTopic(string channel, string topic)
            {
                var channelRegex = new Regex(this.Normalize(this.channel));
                var topicRegex = new Regex(this.Normalize(this.topic));

                return channelRegex.IsMatch(channel) == true
                       && topicRegex.IsMatch(topic);
            }

            public override bool Equals(object obj)
            {
                var other = obj as SubscriberId;

                if (other == null)
                {
                    return false;
                }

                return (other.id == this.id) && (other.channel == this.channel) && (other.topic == this.topic);
            }

            public override int GetHashCode()
            {
                return this.hash;
            }

            public bool PassesCondition(Envelope env)
            {
                return this.condition(env);
            }
        }

        class DisposableSubscription : IDisposable
        {
            private readonly SubscriberId id;
            private readonly IDictionary<SubscriberId, Action< Envelope>> subscribers;

            public DisposableSubscription(SubscriberId id, IDictionary<SubscriberId, Action<Envelope>> subscribers)
            {
                this.id = id;
                this.subscribers = subscribers;
            }

            public void Dispose()
            {
                this.subscribers.Remove(this.id);
            }
        }

        private readonly ConcurrentDictionary<SubscriberId, Action<Envelope>> subscribers = new ConcurrentDictionary<SubscriberId, Action<Envelope>>();

        public void Publish(string channel, string topic, object data)
        {
            this.Validate(channel, topic);
            this.PublishAsync(channel, topic, data).GetAwaiter().GetResult();
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

            this.subscribers[id] = subscriber;

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
                    (this.MatchesChannelAndTopic(subscriber.Key, channel, topic) == true) &&
                    (this.PassesCondition(subscriber.Key, env) == true))
                .Select(subscriber => subscriber.Value);
        }
    }
}