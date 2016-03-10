using Postal.NET;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PostalWhen.NET
{
    public sealed class CompositionBox : ICompositionBox
    {
        internal sealed class Condition
        {
            private readonly string channel;
            private readonly string topic;
            private readonly Func<Envelope, bool> condition;

            public Condition(string channel, string topic, Func<Envelope, bool> condition)
            {
                this.channel = channel;
                this.topic = topic;

                if (condition == null)
                {
                    condition = (env) => true;
                }

                this.condition = condition;
            }

            private string Normalize(string str)
            {
                return str
                    .Replace(".", "\\.")
                    .Replace("*", ".*");
            }

            public bool MatchesChannelAndTopic(Envelope env)
            {
                var channelRegex = new Regex("^" + this.Normalize(this.channel) + "$");
                var topicRegex = new Regex("^" + this.Normalize(this.topic) + "$");

                var matches = channelRegex.IsMatch(env.Channel) == true && topicRegex.IsMatch(env.Topic);

                return matches && this.condition(env);
            }
        }

        private readonly IBox box;
        private readonly Guid id = Guid.NewGuid();
        private readonly List<Condition> conditions = new List<Condition>();
        private readonly IDisposable subscription;
        private Action<Envelope> subscriber;
        private int index;
        private DateTime startTime;
        private TimeSpan? time;

        public CompositionBox(IBox box)
        {
            this.box = box;
            this.subscription = this.box.Subscribe("*", "*", this.OnReceive);
        }

        private void OnReceive(Envelope env)
        {
            if (this.conditions[this.index].MatchesChannelAndTopic(env) == true)
            {
                if (this.index == 0)
                {
                    this.startTime = DateTime.UtcNow;
                }

                this.index++;

                if (this.index == this.conditions.Count)
                {
                    if ((this.time == null) || ((DateTime.UtcNow - this.startTime) < this.time))
                    {
                        this.subscriber(env);
                    }
                }
                else
                {
                    return;
                }
            }

            this.index = 0;
        }

        public ICompositionBox InTime(TimeSpan time)
        {
            this.time = time;

            return this;
        }

        public ICompositionBox And(string channel, string topic, Func<Envelope, bool> condition = null)
        {
            if (string.IsNullOrWhiteSpace(channel) == true)
            {
                throw new ArgumentNullException("channel");
            }

            if (string.IsNullOrWhiteSpace(topic) == true)
            {
                throw new ArgumentNullException("topic");
            }

            this.conditions.Add(new Condition(channel, topic, condition));

            return this;
        }

        public IDisposable Subscribe(Action<Envelope> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            if (this.conditions.Count == 0)
            {
                throw new InvalidOperationException("Missing conditions");
            }

            this.subscriber = subscriber;
            return this.subscription;
        }
    }
}
