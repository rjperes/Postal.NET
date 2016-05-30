using System;
using System.Threading.Tasks;

namespace PostalConventionsNET
{
    /// <summary>
    /// A Postal.NET conventional box.
    /// </summary>
    public interface IConventionsBox
    {
        /// <summary>
        /// Publishes a message based upon conventions.
        /// </summary>
        /// <typeparam name="T">A message type.</typeparam>
        /// <param name="data">A message.</param>
        void Publish<T>(T data);

        /// <summary>
        /// Publishes a message based upon conventions asynchronously.
        /// </summary>
        /// <typeparam name="T">A message type.</typeparam>
        /// <param name="data">A message.</param>
        /// <returns>A task.</returns>
        Task PublishAsync<T>(T data);

        /// <summary>
        /// Subscribes to topics based on conventions.
        /// </summary>
        /// <typeparam name="T">A message type.</typeparam>
        /// <param name="subscriber">A subscriber.</param>
        /// <returns>A subscription.</returns>
        IDisposable Subscribe<T>(Action<T> subscriber);

        /// <summary>
        /// Adds a convention for channels.
        /// </summary>
        /// <typeparam name="T">A message type.</typeparam>
        /// <param name="convention">A convention.</param>
        /// <returns>A Postal.NET conventions box implementation.</returns>
        IConventionsBox AddChannelConvention<T>(Func<T, string> convention);

        /// <summary>
        /// Adds a convention for topics.
        /// </summary>
        /// <typeparam name="T">A message type.</typeparam>
        /// <param name="convention">A convention.</param>
        /// <returns>A Postal.NET conventions box implementation.</returns>
        IConventionsBox AddTopicConvention<T>(Func<T, string> convention);

        /// <summary>
        /// Adds a convention for message types.
        /// </summary>
        /// <typeparam name="T">A message type.</typeparam>
        /// <param name="convention">A convention.</param>
        /// <returns>A Postal.NET conventions box implementation.</returns>
        IConventionsBox AddConditionConvention<T>(Func<T, bool> convention);
    }
}
