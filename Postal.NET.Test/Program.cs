using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using PostalNET.Conventions;
using PostalNET.RX;
using PostalNET.When;
using PostalNET.RequestResponse;
using PostalNET.Interceptor;

namespace PostalNET.Test
{
    static class Program
    {
        class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        class DummyBox : IBox
        {
            public Task PublishAsync(string channel, string topic, object data, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(0);
            }

            public IDisposable Subscribe(string channel, string topic, Action<Envelope> subscriber, Func<Envelope, bool> condition = null)
            {
                return new DummyDisposable();
            }
        }

        class DummyHandler : IHandler<string>, IAsyncHandler<string>
        {
            private readonly EventWaitHandle _evt;

            public DummyHandler(EventWaitHandle evt)
            {
                _evt = evt;
            }

            public void Handle(string msg)
            {
                Console.WriteLine(msg);

                _evt.Set();
            }

            public Task HandleAsync(string msg)
            {
                Console.WriteLine(msg);

                _evt.Set();

                return Task.FromResult(0);
            }
        }

        static async Task TestHandler()
        {
            using var evt = new ManualResetEvent(false);
            var handler = new DummyHandler(evt);

            using (Postal.Box.AddHandler<string>(handler, "channel", "topic"))
            {
                await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");

                evt.WaitOne();
            }
        }

        static async Task TestAsyncHandler()
        {
            using var evt = new ManualResetEvent(false);
            var handler = new DummyHandler(evt);

            using (Postal.Box.AddAsyncHandler<string>(handler, "channel", "topic"))
            {
                await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");

                evt.WaitOne();
            }
        }

        static void TestFactory()
        {
            Postal.Factory = () => new DummyBox();
            var isDummy = Postal.Box is DummyBox;
        }

        static async Task TestOnce()
        {
            Postal.Box.Once("channel", "topic", (env) => Console.WriteLine(env.Data));
            await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");
            await Postal.Box.PublishAsync("channel", "topic", "Does not appear!");
        }

        static void TestRequestResponse()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) =>
            {
                if (env.IsRequestResponse() == true)
                {
                    var data = env.Unwrap<string>();
                    Postal.Box.Reply(env, string.Join(string.Empty, data.Reverse()));
                }
            }))
            {
                var response = Postal.Box.Request("channel", "topic", "Hello, World!");
            }
        }

        static async Task TestMultipleSubscriptions()
        {
            using (Postal.Box.SubscribeMultiple("channel", "topic1, topic2", (env) => Console.WriteLine(env.Data)))
            {
                await Postal.Box.PublishAsync("channel", "topic1", "Hello, World!");
            }

            await Postal.Box.PublishAsync("channel", "topic1", "Does not appear!");
        }

        static async Task TestDifferentSubscriptions()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine("Got: " + env.Data)))
            using (Postal.Box.Subscribe("channel2", "topic2", (env) => Console.WriteLine("Didn't got: " + env.Data)))
            {
                await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");
            }
        }

        static async Task TestDisposition()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
            {
                await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");
            }

            await Postal.Box.PublishAsync("channel", "topic", "Does not appear!");
        }

        static async Task TestCatchAll()
        {
            using (Postal.Box.Subscribe("*", "*", (env) => Console.WriteLine("Catch all!")))
            {
                using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
                {
                    await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");
                }
            }
        }

        static async Task TestCatchSome()
        {
            using (Postal.Box.Subscribe("c*", "t*", (env) => Console.WriteLine("Catch some!")))
            {
                using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
                {
                    await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");
                }
            }
        }

        static async Task TestFilter()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data), (env) => env.Data is int))
            {
                await Postal.Box.PublishAsync("channel", "topic", "Does not show!");
                await Postal.Box.PublishAsync("channel", "topic", 12345);
            }
        }

        static async Task TestAsync()
        {
            using (var evt = new ManualResetEvent(false))
            using (Postal.Box.Subscribe("channel", "topic", (env) =>
            {
                Console.WriteLine(env.Data);
                evt.Set();
            }))
            {
                await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");

                evt.WaitOne();
            }
        }

        static async Task TestFluent()
        {
            using (Postal.Box.AnyChannelAndTopic().Subscribe((env) => Console.WriteLine("Catch all!")))
            {
                using (Postal.Box.Channel("channel").Topic("topic").Subscribe((env) => Console.WriteLine(env.Data)))
                {
                    await Postal.Box.Channel("channel").Topic("topic").PublishAsync("Hello, World!");
                }
            }
        }

        static async Task TestExtensions()
        {
            using (Postal.Box.Subscribe<string>("channel", "topic", data => Console.WriteLine(data)))
            {
                await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");
            }
        }

        static async Task TestReactive()
        {
            var observable = Postal.Box.Observe("channel", "topic");

            using (observable as IDisposable)
            {
                using (observable.Subscribe((env) => Console.WriteLine(env.Data), (ex) => { }, () => { }))
                {
                    await Postal.Box.PublishAsync("channel", "topic", "Hello, World!");
                }
            }
        }

        static async Task TestReactiveBuffered()
        {
            var observable = Postal.Box.Observe("channel", "topic");

            using (observable as IDisposable)
            {
                using (observable.Buffer(5).Subscribe((env) => Console.WriteLine("Got: " + env.Count), (ex) => { }, () => { }))
                {
                    await Postal.Box.PublishAsync("channel", "topic", "Hello, World 1!");
                    await Postal.Box.PublishAsync("channel", "topic", "Hello, World 2!");
                    await Postal.Box.PublishAsync("channel", "topic", "Hello, World 3!");
                    await Postal.Box.PublishAsync("channel", "topic", "Hello, World 4!");
                    await Postal.Box.PublishAsync("channel", "topic", "Hello, World 5!");
                }
            }
        }

        static async Task TestConventions()
        {
            var conventionsBox = Postal
                .Box
                .WithConventions()
                .AddChannelConvention<string>(x => x)
                .AddTopicConvention<string>(x => x);

            using (conventionsBox.Subscribe<string>(x => Console.WriteLine(x)))
            {
                await conventionsBox.PublishAsync<string>("Hello, World!");
            }
        }

        static async Task TestComposition()
        {
            using (Postal
                .Box
                .When("channel1", "topic1")
                .And("channel2", "topic2")
                .Subscribe(env => Console.WriteLine(env.Data)))
            {
                await Postal.Box.PublishAsync("channel1", "topic1", "Will not show");
                await Postal.Box.PublishAsync("channel3", "topic3", "Will not show");
                await Postal.Box.PublishAsync("channel2", "topic2", "Hello, World!");
            }
        }

        static async Task TestTimedComposition()
        {
            using (Postal
                .Box
                .When("channel1", "topic1")
                .And("channel2", "topic2")
                .InTime(TimeSpan.FromSeconds(5))
                .Subscribe(env => Console.WriteLine(env.Data)))
            {
                await Postal.Box.PublishAsync("channel1", "topic1", "Will not show");

                Thread.Sleep(6 * 1000);

                await Postal.Box.PublishAsync("channel2", "topic2", "Will not show too");
            }
        }

        static async Task TestInterruptedComposition()
        {
            using (Postal
                .Box
                .When("channel1", "topic1")
                .And("channel2", "topic2")
                .Subscribe(env => Console.WriteLine(env.Data)))
            {
                await Postal.Box.PublishAsync("channel1", "topic1", "Will not show");
                await Postal.Box.PublishAsync("channel3", "topic3", "Will not show");
                await Postal.Box.PublishAsync("channel2", "topic2", "Will not show");
            }
        }

        static async Task TestConditionalComposition()
        {
            using (Postal
                .Box
                .When("channel1", "topic1", env => env.Data is int)
                .And("channel2", "topic2")
                .Subscribe(env => Console.WriteLine(env.Data)))
            {
                await Postal.Box.PublishAsync("channel1", "topic1", 1);
                await Postal.Box.PublishAsync("channel2", "topic2", "Hello, World!");
            }
        }

        static async Task TestInterception()
        {
            using var before = new ManualResetEvent(false);
            using var after = new ManualResetEvent(false);
            var box = Postal
                .Box
                .InterceptWith(env => { before.Set(); }, env => { after.Set(); });

            using (box.Subscribe("channel", "topic", env => { }))
            {
                await box.PublishAsync("channel", "topic", "Hello, World!");

                WaitHandle.WaitAll([before, after]);
            }
        }

        static async Task Main()
        {
            //These are not unit tests, just samples of how to use Postal.NET
            TestFactory();
            await TestHandler();
            await TestAsyncHandler();
            await TestOnce();
            TestRequestResponse();
            await TestMultipleSubscriptions();
            await TestConditionalComposition();
            await TestInterruptedComposition();
            await TestTimedComposition();
            await TestComposition();
            await TestDifferentSubscriptions();
            await TestExtensions();
            await TestConventions();
            await TestFluent();
            await TestReactive();
            await TestReactiveBuffered();
            await TestAsync();
            await TestFilter();
            await TestDisposition();
            await TestCatchAll();
            await TestCatchSome();
            await TestInterception();

            Console.ReadLine();
        }
    }
}
