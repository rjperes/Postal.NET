using PostalNET;
using System;
using System.Collections.Generic;

namespace PostalRXNET
{
    public static class ObservableExtensions
    {
        class DisposableObserver : IDisposable, IObserver<Envelope>
        {
            private readonly IObserver<Envelope> _observer;
            private readonly ICollection<IObserver<Envelope>> _subscribers;

            public DisposableObserver(IObserver<Envelope> observer, ICollection<IObserver<Envelope>> subscribers)
            {
                this._observer = observer;
                this._subscribers = subscribers;
            }

            public void Dispose()
            {
                this.OnCompleted();
                this._subscribers.Remove(this._observer);
            }

            public void OnNext(Envelope value)
            {
                this._observer.OnNext(value);
            }

            public void OnError(Exception error)
            {
                this._observer.OnError(error);
            }

            public void OnCompleted()
            {
                this._observer.OnCompleted();
            }
        }

        class PostalObservable : IObservable<Envelope>, IDisposable
        {
            private readonly LinkedList<IObserver<Envelope>> _subscribers = new LinkedList<IObserver<Envelope>>();
            private readonly IBox _box;
            private IDisposable _subscription;

            public PostalObservable(ITopic topic)
            {
                if (topic == null)
                {
                    throw new ArgumentNullException(nameof(topic));
                }

                this._subscription = topic.Subscribe(this.Notification);
            }

            public PostalObservable(IBox box, string channel, string topic)
            {
                if (box == null)
                {
                    throw new ArgumentNullException(nameof(box));
                }

                this._box = box;
                this._subscription = this._box.Subscribe(channel, topic, this.Notification);
            }

            private void Notification(Envelope envelope)
            {
                foreach (var observer in this._subscribers)
                {
                    observer.OnNext(envelope);
                }
            }

            public IDisposable Subscribe(IObserver<Envelope> observer)
            {
                if (observer == null)
                {
                    throw new ArgumentNullException(nameof(observer));
                }

                _subscribers.AddLast(observer);

                return new DisposableObserver(observer, this._subscribers);
            }

            public void Dispose()
            {
                foreach (var observer in this._subscribers)
                {
                    observer.OnCompleted();
                }

                this._subscribers.Clear();
                this._subscription.Dispose();
            }
        }

        /// <summary>
        /// Observes a topic.
        /// </summary>
        /// <param name="topic">A topic.</param>
        /// <returns>An observable.</returns>
        public static IObservable<Envelope> Observe(ITopic topic)
        {
            return new PostalObservable(topic);
        }

        /// <summary>
        /// Observes a topic.
        /// </summary>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <returns>An observable.</returns>
        public static IObservable<Envelope> Observe(this IBox box, string channel, string topic)
        {
            return new PostalObservable(box, channel, topic);
        }

        /// <summary>
        /// Observes a topic.
        /// </summary>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <param name="observer">An observer.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable Observe(this IBox box, string channel, string topic, IObserver<Envelope> observer)
        {
            return new PostalObservable(box, channel, topic).Subscribe(observer);
        }
    }
}
