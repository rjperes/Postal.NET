using System;
using System.Threading;
using Postal.NET;

namespace PostalRequestResponse.NET
{
    public static class BoxExtensions
    {
        public static T Unwrap<T>(this Envelope env)
        {
            var wrapped = (env.Data is IRequestResponseData) ? (env.Data as IRequestResponseData).Data : env.Data;
            return (T) wrapped;
        }

        public static object Request(this IBox box, string channel, string topic, object data, TimeSpan? delay = null)
        {
            var correlationId = Guid.NewGuid();
            object response = null;

            if (delay == null)
            {
                delay = TimeSpan.FromSeconds(5);
            }

            using (var evt = new ManualResetEvent(false))
            {
                using (var subscription = box.Subscribe(correlationId.ToString(), correlationId.ToString(), (env) =>
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

        public static void Reply(this IBox box, Envelope env, object data)
        {
            var rrData = env.Data as IRequestResponseData;

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
