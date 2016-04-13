using System;
using System.Threading.Tasks;

namespace PostalNET
{
    /// <summary>
    /// An event topic.
    /// </summary>
    public interface ITopic
    {
        /// <summary>
        /// Subscribes to events conditionally.
        /// </summary>
        /// <param name="subscriber">The subscriber action.</param>
        /// <param name="condition">The condition.</param>
        /// <returns>A subscription.</returns>
        IDisposable SubscribeWhen(Action<Envelope> subscriber, Func<Envelope, bool> condition);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="subscriber">The subscriber action.</param>
        /// <returns>A subscription.</returns>
        IDisposable Subscribe(Action<Envelope> subscriber);

        /// <summary>
        /// Publishes an event synchronously.
        /// </summary>
        /// <param name="data">The event payload.</param>
        void Publish(object data);
        /// <summary>
        /// Publishes an event asynchronously.
        /// </summary>
        /// <param name="data">The event payload.</param>
        /// <returns>A promise.</returns>
        Task PublishAsync(object data);
    }
}