using System;

namespace Postal.NET
{
    public static class BoxExtensions
    {
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
