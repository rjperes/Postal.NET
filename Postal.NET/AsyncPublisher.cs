using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PostalNET
{
    public class AsyncPublisher : IPublisher
    {
        public static readonly IPublisher Instance = new AsyncPublisher();

        public async Task PublishAsync(IEnumerable<Action<Envelope>> destinations, Envelope envelope, CancellationToken cancellationToken = default)
        {
            var task = Task.CompletedTask;

            foreach (var subscriber in destinations)
            {
                task = task.ContinueWith((_, state) =>
                {
                    if (state is CancellationToken ct)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    subscriber(envelope);
                }, cancellationToken, cancellationToken);
            }

            await task;
        }
    }
}