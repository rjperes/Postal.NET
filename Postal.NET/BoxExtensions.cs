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

        /// <summary>
        /// Subscribes to multiple events at the same time.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="channels">A list of event channels, separated by commas.</param>
        /// <param name="topics">A list of event topics, separated by commas.</param>
        /// <param name="subscriber">A subscriber action.</param>
        /// <param name="condition">An optional filtering condition.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Subscribes to an event only once, then unsubscribes.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="channel">The event channel.</param>
        /// <param name="topic">The event topic.</param>
        /// <param name="subscriber">A subscriber action.</param>
        /// <param name="condition">An optional filtering condition.</param>
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

        /// <summary>
        /// Publishes multiple events at the same time.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="channel">The event channel.</param>
        /// <param name="topic">The event topic.</param>
        /// <param name="datas">The event payload.</param>
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

        /// <summary>
        /// Publishes multiple events at the same time.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="channel">The event channel.</param>
        /// <param name="topic">The event topic.</param>
        /// <param name="factory">An event payload factory.</param>
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

        /// <summary>
        /// Subscribes to events with a given payload type.
        /// </summary>
        /// <typeparam name="T">The event payload type.</typeparam>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="channel">The event channel.</param>
        /// <param name="topic">The event topic.</param>
        /// <param name="subscriber">A subscriber action.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable Subscribe<T>(this IBox box, string channel, string topic, Action<T> subscriber)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return box.Subscribe(channel, topic, (env) => subscriber((T) env.Data), (env) => env.Data is T);
        }

        /// <summary>
        /// Returns an event channel by its name.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="channel">The event channel.</param>
        /// <returns>The channel.</returns>
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

        /// <summary>
        /// Returns an universal topic of an universal channel.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <returns>The universal topic of the universal channel.</returns>
        public static ITopic AnyChannelAndTopic(this IBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return AnyChannel(box).AnyTopic();
        }

        /// <summary>
        /// Returns an universal channel.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <returns>The universal channel.</returns>
        public static IChannel AnyChannel(this IBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return Channel(box, Postal.All);
        }

        /// <summary>
        /// Returns an universal topic.
        /// </summary>
        /// <param name="channel">The event channel.</param>
        /// <returns>The universal topic of the given channel.</returns>
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
