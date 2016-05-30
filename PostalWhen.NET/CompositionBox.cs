using PostalNET;
using System;
using System.Collections.Generic;

namespace PostalWhenNET
{
    sealed class CompositionBox : ICompositionBox
    {
        internal sealed class Condition
        {
            private readonly string _channel;
            private readonly string _topic;
            private readonly Func<Envelope, bool> _condition;
            private readonly IChannelTopicMatcher _matcher;

            public Condition(string channel, string topic, Func<Envelope, bool> condition, IChannelTopicMatcher matcher)
            {
                this._matcher = matcher;
                this._channel = channel;
                this._topic = topic;

                if (condition == null)
                {
                    condition = (env) => true;
                }

                this._condition = condition;
            }

            public bool MatchesChannelAndTopic(Envelope env)
            {
                return this._matcher.Matches(this._channel, env.Channel) == true
                    && this._matcher.Matches(this._topic, env.Topic) == true;
            }

            public bool MatchesCondition(Envelope env)
            {
                return this._condition(env);
            }
        }

        private readonly IBox _box;
        private readonly List<Condition> _conditions = new List<Condition>();
        private readonly IDisposable _subscription;
        private Action<Envelope> _subscriber;
        private int _index;
        private DateTime _startTime;
        private TimeSpan? _time;
        private readonly IChannelTopicMatcher _matcher;

        public CompositionBox(IBox box, IChannelTopicMatcher matcher = null)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            this._box = box;

            if (matcher == null)
            {
                if (box is IChannelTopicMatcherProvider)
                {
                    matcher = (box as IChannelTopicMatcherProvider).Matcher;
                }
            }

            this._matcher = matcher ?? WildcardChannelTopicMatcher.Instance;
            this._subscription = this._box.Subscribe(Postal.All, Postal.All, this.OnReceive);
        }

        private void OnReceive(Envelope env)
        {
            if (this._conditions[this._index].MatchesChannelAndTopic(env) == true)
            {
                if (this._conditions[this._index].MatchesCondition(env) == true)
                {
                    if (this._index == 0)
                    {
                        this._startTime = DateTime.UtcNow;
                    }

                    this._index++;

                    if (this._index == this._conditions.Count)
                    {
                        if ((this._time == null) || ((DateTime.UtcNow - this._startTime) < this._time))
                        {
                            this._subscriber(env);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    this._index = 0;
                }
            }
        }

        public ICompositionBox InTime(TimeSpan time)
        {
            this._time = time;

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

            this._conditions.Add(new Condition(channel, topic, condition, this._matcher));

            return this;
        }

        public IDisposable Subscribe(Action<Envelope> subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            if (this._conditions.Count == 0)
            {
                throw new InvalidOperationException("Missing conditions");
            }

            this._subscriber = subscriber;
            return this._subscription;
        }
    }
}
