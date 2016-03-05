using PostalConventions.NET;
using PostalRX.NET;
using System;
using System.Threading;

namespace Postal.NET.Test
{
    class Program
    {
        static void TestMultipleSubscriptions()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine("Got: " + env.Data)))
            using (Postal.Box.Subscribe("channel2", "topic2", (env) => Console.WriteLine("Didn't got: " + env.Data)))
            {
                Postal.Box.Publish("channel", "topic", "Hello, World!");
            }
       }

        static void TestDisposition()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
            {
                Postal.Box.Publish("channel", "topic", "Hello, World!");
            }

            Postal.Box.Publish("channel", "topic", "Does not appear!");
        }

        static void TestCatchAll()
        {
            using (Postal.Box.Subscribe("*", "*", (env) => Console.WriteLine("Catch all!")))
            {
                using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
                {
                    Postal.Box.Publish("channel", "topic", "Hello, World!");
                }
            }
        }

        static void TestCatchSome()
        {
            using (Postal.Box.Subscribe("c*", "t*", (env) => Console.WriteLine("Catch some!")))
            {
                using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
                {
                    Postal.Box.Publish("channel", "topic", "Hello, World!");
                }
            }
        }

        static void TestFilter()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data), (env) => env.Data is int))
            {
                Postal.Box.Publish("channel", "topic", "Does not show!");
                Postal.Box.Publish("channel", "topic", 12345);
            }
        }

        static void TestAsync()
        {
            using (var evt = new ManualResetEvent(false))
            using (Postal.Box.Subscribe("channel", "topic", (env) => {
                Console.WriteLine(env.Data);
                evt.Set();
            }))
            {
                Postal.Box.PublishAsync("channel", "topic", "Hello, World!");

                evt.WaitOne();
            }
        }

        static void TestFluent()
        {
            using (Postal.Box.AnyChannelAndTopic().Subscribe((env) => Console.WriteLine("Catch all!")))
            {
                using (Postal.Box.Channel("channel").Topic("topic").Subscribe((env) => Console.WriteLine(env.Data)))
                {
                    Postal.Box.Channel("channel").Topic("topic").Publish("Hello, World!");
                }
            }
        }

        static void TestExtensions()
        {
            using (Postal.Box.Subscribe<string>("channel", "topic", data => Console.WriteLine(data)))
            {
                Postal.Box.Publish("channel", "topic", "Hello, World!");
            }
        }

        static void TestObserver()
        {
            var observable = Postal.Box.Observe("channel", "topic");

            using (observable as IDisposable)
            {
                using (observable.Subscribe((env) => Console.WriteLine(env.Data), (ex) => { }, () => {}))
                {
                    Postal.Box.Publish("channel", "topic", "Hello, World!");
                }
            }
        }

        static void TestConventions()
        {
            var conventionsBox = Postal
                .Box
                .WithConventions()
                .AddChannelConvention<string>(x => x)
                .AddTopicConvention<string>(x => x);

            using (conventionsBox.Subscribe<string>(x => Console.WriteLine(x)))
            {
                conventionsBox.Publish<string>("Hello, World!");
            }
        }

        static void Main(string[] args)
        {
            TestMultipleSubscriptions();
            TestExtensions();
            TestConventions();
            TestFluent();
            TestObserver();
            TestAsync();
            TestFilter();
            TestDisposition();
            TestCatchAll();
            TestCatchSome();

            Console.ReadLine();
        }
    }
}
