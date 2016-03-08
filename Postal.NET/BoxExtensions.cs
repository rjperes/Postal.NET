using System;

namespace Postal.NET
{
    public static class BoxExtensions
    {
        public static IDisposable Subscribe<T>(this IBox box, string channel, string topic, Action<T> subscriber)
        {
            return box.Subscribe(channel, topic, (env) => subscriber((T) env.Data), (env) => env.Data is T);
        }

        public static IChannel Channel(this IBox box, string channel)
        {
            if (string.IsNullOrWhiteSpace(channel) == true)
            {
                throw new ArgumentNullException("channel");
            }

            return new Channel(box, channel);
        }

        public static ITopic AnyChannelAndTopic(this IBox box)
        {
            return AnyChannel(box).AnyTopic();
        }

        public static IChannel AnyChannel(this IBox box)
        {
            return Channel(box, "*");
        }

        public static ITopic AnyTopic(this IChannel channel)
        {
            return channel.Topic("*");
        }
    }
}
