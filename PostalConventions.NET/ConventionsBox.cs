using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PostalNET.Conventions
{
    sealed class ConventionsBox : IConventionsBox, IBox
    {
        private readonly IBox _box;
        private readonly Dictionary<Type, Func<object, string>> _channelConventions = new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<Type, Func<object, string>> _topicConventions = new Dictionary<Type, Func<object, string>>();
        private Func<object, bool> _condition;

        public ConventionsBox(IBox box)
        {
            this._box = box;
            this._condition = (env) => true;
        }

        public IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
        {
            return this._box.Subscribe(channel, topic, subscriber, (env) => condition(env) && this._condition(env));
        }

        public void Publish(string channel, string topic, object data)
        {
            this._box.Publish(channel, topic, data);
        }

        public async Task PublishAsync(string channel, string topic, object data)
        {
            await this._box.PublishAsync(channel, topic, data);
        }

        public IConventionsBox AddConditionConvention<T>(Func<T, bool> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException("convention");
            }

            this._condition = (data) => (data is T) && (convention((T)data));

            return this;
        }

        public IConventionsBox AddChannelConvention<T>(Func<T, string> convention)
        {
            this._channelConventions[typeof (T)] = data => convention((T)data);
            return this;
        }

        public IConventionsBox AddTopicConvention<T>(Func<T, string> convention)
        {
            this._topicConventions[typeof (T)] = data => convention((T)data);
            return this;
        }

        public IDisposable Subscribe<T>(Action<T> subscriber)
        {
            var subscription = this._box.Subscribe(Postal.All, Postal.All, (env) =>
            {
                if (this._condition(env) == true)
                {
                    if (env.Data is T)
                    {
                        subscriber((T)env.Data);
                    }
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

        private int Distance(Type source, Type target)
        {
            if (target.IsAssignableFrom(source) == false)
            {
                return int.MaxValue;
            }

            if (source.GetTypeInfo().IsInterface == true)
            {
                return int.MaxValue / 2;
            }

            var current = source;
            var distance = 0;

            while (current != typeof(object))
            {
                if (target == current)
                {
                    break;
                }

                current = current.GetTypeInfo().BaseType;
                distance++;
            }

            return distance;
        }

        private string Find<T>(T data, Dictionary<Type, Func<object, string>> conventions)
        {
            var convention = (from c in conventions
                let d = this.Distance(c.Key, typeof (T))
                where d < int.MaxValue
                orderby d
                select c.Value).FirstOrDefault();

            if (convention == null)
            {
                throw new InvalidOperationException(string.Format("No convention for data type {0}", typeof(T)));
            }

            return convention(data);
        }

        private string FindTopic<T>(T data)
        {
            return this.Find<T>(data, this._topicConventions);
        }

        private string FindChannel<T>(T data)
        {
            return this.Find<T>(data, this._channelConventions);
        }
    }
}
