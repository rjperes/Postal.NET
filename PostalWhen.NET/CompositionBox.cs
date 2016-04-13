using PostalNET;
using System;
using System.Collections.Generic;

namespace PostalWhenNET
{
    public sealed class CompositionBox : ICompositionBox
    {
        internal sealed class Condition
        {
            private readonly string channel;
            private readonly string topic;
            private readonly Func<Envelope, bool> condition;
            private readonly IChannelTopicMatcher matcher;

            public Condition(string channel, string topic, Func<Envelope, bool> condition, IChannelTopicMatcher matcher)
            {
                this.matcher = matcher;
                this.channel = channel;
                this.topic = topic;

                if (condition == null)
                {
                    condition = (env) => true;
                }

                this.condition = condition;
            }

            public bool MatchesChannelAndTopic(Envelope env)
            {
                return this.matcher.Matches(this.channel, env.Channel) == true
                    && this.matcher.Matches(this.topic, env.Topic) == true;
            }

            public bool MatchesCondition(Envelope env)
            {
                return this.condition(env);
            }
        }

        private readonly IBox box;
        private readonly List<Condition> conditions = new List<Condition>();
        private readonly IDisposable subscription;
        private Action<Envelope> subscriber;
        private int index;
        private DateTime startTime;
        private TimeSpan? time;
        private IChannelTopicMatcher matcher;

        public CompositionBox(IBox box, IChannelTopicMatcher matcher = null)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            this.box = box;
            //TODO: somehow get the matcher from the box
            this.matcher = matcher ?? WildcardChannelTopicMatcher.Instance;
            this.subscription = this.box.Subscribe(Postal.All, Postal.All, this.OnReceive);
        }

        private void OnReceive(Envelope env)
        {
            if (this.conditions[this.index].MatchesChannelAndTopic(env) == true)
            {
                if (this.conditions[this.index].MatchesCondition(env) == true)
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
                else
                {
                    this.index = 0;
                }
            }
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

            this.conditions.Add(new Condition(channel, topic, condition, this.matcher));

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
