using System;

namespace Postal.NET
{
    /// <summary>
    /// An event envelope.
    /// </summary>
    public sealed class Envelope
    {
        public Envelope(string channel, string topic, object data)
        {
            this.Timestamp = DateTime.UtcNow;
            this.Channel = channel;
            this.Topic = topic;
            this.Data = data;
        }

        /// <summary>
        /// The event timestamp.
        /// </summary>
        public DateTime Timestamp { get; private set; }
        /// <summary>
        /// The event channel.
        /// </summary>
        public string Channel { get; private set; }
        /// <summary>
        /// The event topic.
        /// </summary>
        public string Topic { get; private set; }
        /// <summary>
        /// The event payload.
        /// </summary>
        public object Data { get; private set; }
    }
}