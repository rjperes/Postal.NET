using System;
using System.Threading.Tasks;

namespace PostalNET
{
    /// <summary>
    /// Actual implementation contract for Postal.NET.
    /// </summary>
    public interface ISubscriberStore
    {
        /// <summary>
        /// How to match channels and topics.
        /// </summary>
        IChannelTopicMatcher Matcher { get; set; }

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="channel">The event channel.</param>
        /// <param name="topic">The event topic.</param>
        /// <param name="subscriber">The subscriber action.</param>
        /// <param name="condition">An optional filtering condition.</param>
        /// <returns></returns>
        IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition);

        /// <summary>
        /// Publishes an event asynchronously.
        /// </summary>
        /// <param name="env">The event envelope.</param>
        /// <returns>A promise.</returns>
        Task PublishAsync(Envelope env);

        /// <summary>
        /// Publishes an event synchronously.
        /// </summary>
        /// <param name="env">The event envelope.</param>
        void Publish(Envelope env);
    }
}
