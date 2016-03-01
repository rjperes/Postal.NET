using System;

namespace Postal.NET
{
    public sealed class Envelope
    {
        public Envelope(string channel, string topic, object data)
        {
            this.Timestamp = DateTime.UtcNow;
            this.Channel = channel;
            this.Topic = topic;
            this.Data = data;
        }

        public DateTime Timestamp { get; private set; }
        public string Channel { get; private set; }
        public string Topic { get; private set; }
        public object Data { get; private set; }
    }
}