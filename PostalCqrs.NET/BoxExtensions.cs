using PostalNET;
using System;
using PostalRequestResponseNET;
using System.Threading.Tasks;

namespace PostalCqrsNET
{
    /// <summary>
    /// Extensions for working with commands and queries (CQRS).
    /// Note:
    ///     - Commands are always asynchronous
    ///     - Queries are always synchronous and return a value
    /// </summary>
    public static class BoxExtensions
    {
        /// <summary>
        /// Sends a query and synchronously waits for a response.
        /// </summary>
        /// <typeparam name="TQuery">A query type.</typeparam>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <param name="query">A query.</param>
        /// <param name="delay">An optional delay.</param>
        /// <returns>A response.</returns>
        public static object RequestQuery<TQuery>(this IBox box, string channel, string topic, TQuery query, TimeSpan? delay = null) where TQuery : IQuery
        {
            return box.Request(channel, topic, query, delay);
        }

        /// <summary>
        /// Sends a query and synchronously waits for a typed response.
        /// </summary>
        /// <typeparam name="TQuery">A query type.</typeparam>
        /// <typeparam name="TResponse">A response type.</typeparam>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <param name="query">A query.</param>
        /// <param name="delay">An optional delay.</param>
        /// <returns>A response.</returns>
        public static TResponse RequestQuery<TQuery, TResponse>(this IBox box, string channel, string topic, TQuery query, TimeSpan? delay = null) where TQuery : IQuery
        {
            return (TResponse)RequestQuery<TQuery>(box, channel, topic, query, delay);
        }

        /// <summary>
        /// Sends a command asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">A command type.</typeparam>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <param name="command">A command.</param>
        /// <returns>A task.</returns>
        public static Task CommandAsync<TCommand>(this IBox box, string channel, string topic, TCommand command) where TCommand : ICommand
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            return box.PublishAsync(channel, topic, command);
        }

        /// <summary>
        /// Sends a command over a topic asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">A command type.</typeparam>
        /// <param name="topic">A topic.</param>
        /// <param name="command">A command.</param>
        /// <returns>A task.</returns>
        public static Task CommandAsync<TCommand>(this ITopic topic, TCommand command) where TCommand : ICommand
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            return topic.PublishAsync(command);
        }

        /// <summary>
        /// Handles messages of a given command type.
        /// </summary>
        /// <typeparam name="TCommand">A command type.</typeparam>
        /// <param name="topic">A topic.</param>
        /// <param name="handler">An handler.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable HandleCommand<TCommand>(this ITopic topic, IHandler<TCommand> handler) where TCommand : ICommand
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return topic.SubscribeWhen((env) => handler.Handle((TCommand)env.Data), (env) => env.Data is TCommand);
        }

        /// <summary>
        /// Handles messages of a given query type.
        /// </summary>
        /// <typeparam name="TQuery">A query type.</typeparam>
        /// <param name="topic">A topic.</param>
        /// <param name="handler">An handler.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable HandleQuery<TQuery>(this ITopic topic, IHandler<TQuery> handler) where TQuery : IQuery
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return topic.SubscribeWhen((env) => handler.Handle((TQuery)env.Data), (env) => env.Data is TQuery);
        }

        /// <summary>
        /// Handles messages of a given command type asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">A command type.</typeparam>
        /// <param name="topic">A topic.</param>
        /// <param name="handler">An asynchronous handler.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable HandleCommandAsync<TCommand>(this ITopic topic, IAsyncHandler<TCommand> handler) where TCommand : ICommand
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return topic.SubscribeWhen((env) => handler.HandleAsync((TCommand)env.Data), (env) => env.Data is TCommand);
        }

        /// <summary>
        /// Handles messages of a given query type asynchronously.
        /// </summary>
        /// <typeparam name="TQuery">A query type.</typeparam>
        /// <param name="topic">A topic.</param>
        /// <param name="handler">An asynchronous handler.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable HandleQueryAsync<TQuery>(this ITopic topic, IAsyncHandler<TQuery> handler) where TQuery : IQuery
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return topic.SubscribeWhen((env) => handler.HandleAsync((TQuery)env.Data), (env) => env.Data is TQuery);
        }

        /// <summary>
        /// Handles messages of a given command type.
        /// </summary>
        /// <typeparam name="TCommand">A command type.</typeparam>
        /// <param name="handler">An handler.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable HandleCommand<TCommand>(this IBox box, IHandler<TCommand> handler, string channel = null, string topic = null) where TCommand : ICommand
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return box.AddHandler(handler, channel, topic);
        }

        /// <summary>
        /// Handles messages of a given query type.
        /// </summary>
        /// <typeparam name="TQuery">A query type.</typeparam>
        /// <param name="handler">An handler.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable HandleQuery<TQuery>(this IBox box, IHandler<TQuery> handler, string channel = null, string topic = null) where TQuery : IQuery
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return box.AddHandler(handler, channel, topic);
        }

        /// <summary>
        /// Handles messages of a given command type asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">A command type.</typeparam>
        /// <param name="handler">An asynchronous handler.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable HandleCommandAsync<TCommand>(this IBox box, IAsyncHandler<TCommand> handler, string channel = null, string topic = null) where TCommand : ICommand
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return box.AddAsyncHandler(handler, channel, topic);
        }

        /// <summary>
        /// Handles messages of a given query type asynchronously.
        /// </summary>
        /// <typeparam name="TQuery">A query type.</typeparam>
        /// <param name="handler">An asynchronous handler.</param>
        /// <param name="channel">A channel.</param>
        /// <param name="topic">A topic.</param>
        /// <returns>A subscription.</returns>
        public static IDisposable HandleQueryAsync<TQuery>(this IBox box, IAsyncHandler<TQuery> handler, string channel = null, string topic = null) where TQuery : IQuery
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return box.AddAsyncHandler(handler, channel, topic);
        }
    }
}
