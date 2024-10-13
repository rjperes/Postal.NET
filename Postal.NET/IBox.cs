﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace PostalNET
{
    /// <summary>
    /// The core contract for Postal.NET.
    /// </summary>
    public interface IBox
    {
        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="channel">The event channel.</param>
        /// <param name="topic">The event topic.</param>
        /// <param name="subscriber">The subscriber action.</param>
        /// <param name="condition">An optional filtering condition.</param>
        /// <returns>The subscription.</returns>
        IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null);

        /// <summary>
        /// Publishes an event asynchronously.
        /// </summary>
        /// <param name="channel">The event channel.</param>
        /// <param name="topic">The event topic</param>
        /// <param name="data"></param>
        /// <returns>A promise.</returns>
        Task PublishAsync(string channel, string topic, object data, CancellationToken cancellation = default);
    }
}
