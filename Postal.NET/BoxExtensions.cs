using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PostalNET
{
    public static class BoxExtensions
    {
        class MultiSubscription : IDisposable
        {
            private readonly IEnumerable<IDisposable> _subscriptions;

            public MultiSubscription(IEnumerable<IDisposable> subscriptions)
            {
                ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));
                this._subscriptions = subscriptions;
            }

            public void Dispose()
            {
                foreach (var subscription in this._subscriptions)
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
            ArgumentNullException.ThrowIfNull(box, nameof(box));
            ArgumentException.ThrowIfNullOrWhiteSpace(channels, nameof(channels));
            ArgumentException.ThrowIfNullOrWhiteSpace(topics, nameof(topics));

            var subscriptions = new List<IDisposable>();

            foreach (var channel in channels.Split(',').Select(x => x.Trim()).Distinct())
            {
                foreach (var topic in topics.Split(',').Select(x => x.Trim()).Distinct())
                {
                    subscriptions.Add(box.Subscribe(channel, topic, subscriber, condition));
                }
            }

            if (subscriptions.Count == 0)
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
            ArgumentNullException.ThrowIfNull(box, nameof(box));
            ArgumentException.ThrowIfNullOrWhiteSpace(channel, nameof(channel));
            ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));
            ArgumentNullException.ThrowIfNull(subscriber, nameof(subscriber));

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
        /// <param name="factory">An event payload factory.</param>
        public static async Task MultiPublish(this IBox box, string channel, string topic, Func<object> factory, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(box, nameof(box));
            ArgumentNullException.ThrowIfNull(factory, nameof(factory));
            ArgumentException.ThrowIfNullOrEmpty(channel, nameof(channel));
            ArgumentException.ThrowIfNullOrEmpty(topic, nameof(topic));


            for (var data = factory(); data != null; data = factory())
            {
                await box.PublishAsync(channel, topic, data, cancellationToken);
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
            ArgumentNullException.ThrowIfNull(box, nameof(box));
            ArgumentException.ThrowIfNullOrWhiteSpace(channel, nameof(channel));
            ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));

            return box.Subscribe(channel, topic, (env) => subscriber((T)env.Data), (env) => env.Data is T);
        }

        /// <summary>
        /// Returns an event channel by its name.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="channel">The event channel.</param>
        /// <returns>The channel.</returns>
        public static IChannel Channel(this IBox box, string channel)
        {
            ArgumentNullException.ThrowIfNull(box, nameof(box));
            ArgumentException.ThrowIfNullOrWhiteSpace(channel, nameof(channel));

            return new Channel(box, channel);
        }

        /// <summary>
        /// Returns an universal topic of an universal channel.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <returns>The universal topic of the universal channel.</returns>
        public static ITopic AnyChannelAndTopic(this IBox box)
        {
            ArgumentNullException.ThrowIfNull(box, nameof(box));

            return AnyChannel(box).AnyTopic();
        }

        /// <summary>
        /// Returns an universal channel.
        /// </summary>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <returns>The universal channel.</returns>
        public static IChannel AnyChannel(this IBox box)
        {
            ArgumentNullException.ThrowIfNull(box, nameof(box));

            return Channel(box, Postal.All);
        }

        /// <summary>
        /// Returns an universal topic.
        /// </summary>
        /// <param name="channel">The event channel.</param>
        /// <returns>The universal topic of the given channel.</returns>
        public static ITopic AnyTopic(this IChannel channel)
        {
            ArgumentNullException.ThrowIfNull(channel, nameof(channel));

            return channel.Topic(Postal.All);
        }

        /// <summary>
        /// Adds an event handler.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="channel">An optional channel.</param>
        /// <param name="topic">An optional topic.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable AddHandler<T>(this IBox box, IHandler<T> handler, string channel = null, string topic = null)
        {
            ArgumentNullException.ThrowIfNull(box, nameof(box));
            ArgumentNullException.ThrowIfNull(handler, nameof(handler));

            if (channel == string.Empty)
            {
                channel = null;
            }

            if (topic == string.Empty)
            {
                topic = null;
            }

            return box
                .Channel(channel ?? Postal.All)
                .Topic(topic ?? Postal.All)
                .SubscribeWhen((env) => handler.Handle((T)env.Data), env => env.Data is T);
        }

        /// <summary>
        /// Adds an asynchronous event handler.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="box">A Postal.NET implementation.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="channel">An optional channel.</param>
        /// <param name="topic">An optional topic.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable AddAsyncHandler<T>(this IBox box, IAsyncHandler<T> handler, string channel = null, string topic = null)
        {
            ArgumentNullException.ThrowIfNull(box, nameof(box));

            if (channel == string.Empty)
            {
                channel = null;
            }

            if (topic == string.Empty)
            {
                topic = null;
            }

            return box
                .Channel(channel ?? Postal.All)
                .Topic(topic ?? Postal.All)
                .SubscribeWhen(async (env) => await handler.HandleAsync((T)env.Data), env => env.Data is T);
        }
    }
}
