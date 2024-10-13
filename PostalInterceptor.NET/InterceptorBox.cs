using System;
using System.Threading;
using System.Threading.Tasks;

namespace PostalNET.Interceptor
{
    internal sealed class InterceptorBox : IBox
    {
        private readonly IBox _box;
        private readonly Action<Envelope> _after;
        private readonly Action<Envelope> _before;

        public InterceptorBox(IBox box, Action<Envelope> before, Action<Envelope> after)
        {
            if (box == null)
            {
                throw new ArgumentNullException(nameof(box));
            }

            this._box = box;
            this._after = after;
            this._before = before;
        }

        public async Task PublishAsync(string channel, string topic, object data, CancellationToken cancellationToken = default)
        {
            var env = new Envelope(channel, topic, data);

            if (this._before != null)
            {
                this._before(env);
            }

            await this._box.PublishAsync(channel, topic, data);

            if (this._after != null)
            {
                this._after(env);
            }
        }

        public IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            return this._box.Subscribe(channel, topic, subscriber, condition);
        }
    }
}
