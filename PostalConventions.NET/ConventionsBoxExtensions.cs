using System;
using PostalNET;

namespace PostalConventionsNET
{
    public static class ConventionsBoxExtensions
    {
        /// <summary>
        /// Returns a Postal.NET box implementation that uses conventions.
        /// </summary>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <returns>A Postal.NET box implementation that uses conventions.</returns>
        public static IConventionsBox WithConventions(this IBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException(nameof(box));
            }

            return new ConventionsBox(box);
        }

        /// <summary>
        /// Returns a Postal.NET box implementation that uses a default set of conventions.
        /// </summary>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <returns>A Postal.NET box implementation that uses conventions.</returns>
        public static IConventionsBox WithDefaultConventions(this IBox box)
        {
            return WithConventions(box)
                .AddChannelConvention<object>((data) => data.GetType().Namespace)
                .AddTopicConvention<object>((data) => data.GetType().Name);
        }

        /// <summary>
        /// Returns a Postal.NET box implementation that uses a default set of conventions for typed messages.
        /// </summary>
        /// <typeparam name="T">A message type.</typeparam>
        /// <param name="box">A Postal.NET box implementation.</param>
        /// <returns>A Postal.NET box implementation that uses conventions.</returns>
        public static IConventionsBox WithDefaultConventions<T>(this IBox box)
        {
            return WithConventions(box)
                .AddChannelConvention<T>((data) => typeof(T).Namespace)
                .AddTopicConvention<T>((data) => typeof(T).Name)
                .AddConditionConvention<T>((data) => data is T);
        }
    }
}
