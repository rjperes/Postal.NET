using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PostalNET
{
    /// <summary>
    /// Basic contract for a message publisher.
    /// </summary>
    public interface IPublisher
    {
        Task PublishAsync(IEnumerable<Action<Envelope>> destinations, Envelope envelope, CancellationToken cancellationToken = default);
    }
}
