using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Postal.NET;

namespace PostalConventions.NET
{
    public sealed class ConventionsBox : IConventionsBox, IBox
    {
        private readonly IBox box;
        private readonly Dictionary<Type, Func<object, string>> channelConventions = new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<Type, Func<object, string>> topicConventions = new Dictionary<Type, Func<object, string>>();

        public ConventionsBox(IBox box)
        {
            this.box = box;
        }

        public IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            return this.box.Subscribe(channel, topic, subscriber, condition);
        }

        public void Publish(string channel, string topic, object data)
        {
            this.box.Publish(channel, topic, data);
        }

        public async Task PublishAsync(string channel, string topic, object data)
        {
            await this.box.PublishAsync(channel, topic, data);
        }

        public IConventionsBox AddChannelConvention<T>(Func<T, string> convention)
        {
            this.channelConventions[typeof (T)] = data => convention((T)data);
            return this;
        }

        public IConventionsBox AddTopicConvention<T>(Func<T, string> convention)
        {
            this.topicConventions[typeof (T)] = data => convention((T)data);
            return this;
        }

        public IDisposable Subscribe<T>(Action<T> subscriber)
        {
            if ((this.channelConventions.ContainsKey(typeof (T)) == false) ||
                (this.topicConventions.ContainsKey(typeof (T)) == false))
            {
                throw new InvalidOperationException(string.Format("No convention for data type {0}", typeof(T)));
            }

            var subscription = this.box.Subscribe("*", "*", (env) =>
            {
                if (env.Data is T)
                {
                    subscriber((T) env.Data);
                }
            });

            return subscription;
        }

        public void Publish<T>(T data)
        {
            var channel = this.FindChannel(data);
            var topic = this.FindTopic(data);

            this.Publish(channel, topic, data);
        }

        public async Task PublishAsync<T>(T data)
        {
            var channel = this.FindChannel(data);
            var topic = this.FindTopic(data);

            await this.PublishAsync(channel, topic, data);
        }

        private string Find<T>(T data, Dictionary<Type, Func<object, string>> conventions)
        {
            Func<object, string> convention;

            if (conventions.TryGetValue(typeof (T), out convention) == false)
            {
                throw new InvalidOperationException(string.Format("No convention for data type {0}", typeof(T)));
            }

            return convention(data);
        }

        private string FindTopic<T>(T data)
        {
            return this.Find<T>(data, this.topicConventions);
        }

        private string FindChannel<T>(T data)
        {
            return this.Find<T>(data, this.channelConventions);
        }
    }
}
