using System;

namespace PostalNET
{
    internal sealed class Channel : IChannel
    {
        private readonly IBox _box;
        private readonly string _channel;

        public Channel(IBox box, string channel)
        {
            this._box = box;
            this._channel = channel;
        }

        public ITopic Topic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic) == true)
            {
                throw new ArgumentNullException(nameof(topic));
            }

            return new Topic(this._box, this._channel, topic);
        }

        public override string ToString()
        {
            return this._channel;
        }
    }
}
