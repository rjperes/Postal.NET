using System;
using System.Collections.Generic;
using System.Linq;

namespace Postal.NET
{
    public static class BoxExtensions
    {
        class MultiSubscription : IDisposable
        {
            private readonly IEnumerable<IDisposable> subscriptions;

            public MultiSubscription(IEnumerable<IDisposable> subscriptions)
            {
                this.subscriptions = subscriptions;
            }

            public void Dispose()
            {
                foreach (var subscription in this.subscriptions)
                {
                    subscription.Dispose();
                }
            }
        }

        public static IDisposable SubscribeMultiple(this IBox box, string channels, string topics, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            var subscriptions = new List<IDisposable>();

            foreach (var channel in channels.Split(',').Select(x => x.Trim()).Distinct())
            {
                foreach (var topic in topics.Split(',').Select(x => x.Trim()).Distinct())
                {
                    subscriptions.Add(box.Subscribe(channel, topic, subscriber, condition));
                }
            }

            if (subscriptions.Any() == false)
            {
                throw new InvalidOperationException("No subscriptions supplied");
            }

            return new MultiSubscription(subscriptions);
        }

        public static void Once(this IBox box, string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            IDisposable subscription = null;

            subscription = box.Subscribe(channel, topic, (env) =>
            {
                subscriber(env);

                if (subscription != null)
                {
                    subscription.Dispose();
                    subscription = null;
                }
            }, condition);
        }

        public static void MultiPublish(this IBox box, string channel, string topic, params object[] datas)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            foreach (var data in datas)
            {
                box.Publish(channel, topic, data);
            }
        }

        public static void MultiPublish(this IBox box, string channel, string topic, Func<object> factory)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            for (var data = factory(); data != null; data = factory())
            {
                box.Publish(channel, topic, data);
            }
        }

        public static IDisposable Subscribe<T>(this IBox box, string channel, string topic, Action<T> subscriber)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return box.Subscribe(channel, topic, (env) => subscriber((T) env.Data), (env) => env.Data is T);
        }

        public static IChannel Channel(this IBox box, string channel)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            if (string.IsNullOrWhiteSpace(channel) == true)
            {
                throw new ArgumentNullException("channel");
            }

            return new Channel(box, channel);
        }

        public static ITopic AnyChannelAndTopic(this IBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return AnyChannel(box).AnyTopic();
        }

        public static IChannel AnyChannel(this IBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return Channel(box, Postal.All);
        }

        public static ITopic AnyTopic(this IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            return channel.Topic(Postal.All);
        }
    }
}
