using System;
using System.Threading;
using Postal.NET;

namespace PostalRequestResponse.NET
{
    public static class BoxExtensions
    {
        public static IDisposable SubscribeRequestResponse(this IBox box, string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            if (condition == null)
            {
                condition = (env) => true;
            }

            return box.Subscribe(channel, topic, subscriber, (env) => env.IsRequestResponse() && condition(env));
        }

        public static bool IsRequestResponse(this Envelope env)
        {
            return env.Data is IRequestResponseData;
        }

        public static T Unwrap<T>(this Envelope env)
        {
            var wrapped = env.IsRequestResponse() ? (env.Data as IRequestResponseData).Data : env.Data;
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
