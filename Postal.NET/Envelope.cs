using System;

namespace PostalNET
{
    /// <summary>
    /// An event envelope.
    /// </summary>
    public sealed class Envelope
    {
        public Envelope(string channel, string topic, object data)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(channel, nameof(channel));
            ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));
            this.Timestamp = DateTime.UtcNow;
            this.Channel = channel;
            this.Topic = topic;
            this.Data = data;
        }

        /// <summary>
        /// The event timestamp.
        /// </summary>
        public DateTime Timestamp { get; }
        /// <summary>
        /// The event channel.
        /// </summary>
        public string Channel { get; }
        /// <summary>
        /// The event topic.
        /// </summary>
        public string Topic { get; }
        /// <summary>
        /// The event payload.
        /// </summary>
        public object Data { get; }
    }
}