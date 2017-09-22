using System;
using System.Threading;

namespace PostalNET.RequestResponse
{
    public static class BoxExtensions
    {
        private static readonly TimeSpan _defaultDelay = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Checks if a message is a request-response one.
        /// </summary>
        /// <param name="envelope">A message envelope.</param>
        /// <returns>True if the message is request-response, false otherwise.</returns>
        public static bool IsRequestResponse(this Envelope envelope)
        {
            return envelope.Data is IRequestResponseData;
        }

        /// <summary>
        /// Returns the underlying data in a request-response message.
        /// </summary>
        /// <param name="envelope">A message envelope.</param>
        /// <returns>The message data.</returns>
        public static object Unwrap(this Envelope envelope)
        {
            return Unwrap<object>(envelope);
        }

        /// <summary>
        /// Returns the underlying typed data in a request-response message.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="envelope">A message envelope.</param>
        /// <returns>The typed message data.</returns>
        public static T Unwrap<T>(this Envelope envelope)
        {
            var wrapped = envelope.IsRequestResponse() ? (envelope.Data as IRequestResponseData).Data : envelope.Data;
            return (T) wrapped;
        }

        /// <summary>
        /// Sends a request-response message.
        /// </summary>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <param name="data">The message.</param>
        /// <param name="delay">An optional delay.</param>
        /// <returns>The response.</returns>
        public static object Request(this IBox box, string channel, string topic, object data, TimeSpan? delay = null)
        {
            var correlationId = Guid.NewGuid();
            object response = null;

            delay = delay ?? _defaultDelay;

            using (var evt = new ManualResetEvent(false))
            {
                using (box.Subscribe(correlationId.ToString(), correlationId.ToString(), (env) =>
                {
                    var rrData = env.Data as IRequestResponseData;

                    response = rrData.Data;

                    evt.Set();
                }))
                {
                    box.PublishAsync(channel, topic, new RequestResponseData(data, correlationId));
                    evt.WaitOne(delay.Value);
                }
            }

            return response;
        }

        /// <summary>
        /// Sends a request-response message.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <param name="data">The message.</param>
        /// <param name="delay">An optional delay.</param>
        /// <returns>The typed response.</returns>
        public static T Request<T>(this IBox box, string channel, string topic, object data, TimeSpan? delay = null)
        {
            return (T)Request(box, channel, topic, data, delay);
        }

        /// <summary>
        /// Sends a request-response response.
        /// </summary>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <param name="envelope">The request message envelope.</param>
        /// <param name="data">A message.</param>
        public static void Reply(this IBox box, Envelope envelope, object data)
        {
            var rrData = envelope.Data as IRequestResponseData;

            if (rrData != null)
            {
                box.PublishAsync(rrData.CorrelationId.ToString(), rrData.CorrelationId.ToString(), new RequestResponseData(data, rrData.CorrelationId));
            }
            else
            {
                throw new ArgumentException("Message is not a request-response one", "env");
            }
        }
    }
}
