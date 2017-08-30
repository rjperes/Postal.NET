using System;
using System.Collections.Generic;

namespace PostalNET
{
    /// <summary>
    /// Basic contract for a message publisher.
    /// </summary>
    public interface IPublisher
    {
        void Publish(IEnumerable<Action<Envelope>> destinations, Envelope envelope);
    }
}
