using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostalNET
{
    public class AsyncPublisher : IPublisher
    {
        public static readonly IPublisher Instance = new AsyncPublisher();

        public void Publish(IEnumerable<Action<Envelope>> destinations, Envelope envelope)
        {
            this
                .PublishAsync(destinations, envelope)
                .GetAwaiter()
                .GetResult();
        }

        public async Task PublishAsync(IEnumerable<Action<Envelope>> destinations, Envelope envelope)
        {
            foreach (var subscriber in destinations.AsParallel())
            {
                await Task.Run(() => subscriber(envelope));
            }
        }
    }
}