using System;

namespace PostalNET.Interceptor
{
    public static class PostalExtensions
    {
        /// <summary>
        /// Adds interception to an <see cref="IBox"/>.
        /// </summary>
        /// <param name="box">An existing box instance.</param>
        /// <param name="before">A before handler.</param>
        /// <param name="after">An after handler.</param>
        /// <returns>A wrapped box instance.</returns>
        public static IBox InterceptWith(this IBox box, Action<Envelope> before, Action<Envelope> after)
        {
            return new InterceptorBox(box, before, after);
        }

        /// <summary>
        /// Adds before interception to an <see cref="IBox"/>.
        /// </summary>
        /// <param name="box">An existing box instance.</param>
        /// <param name="before">A before handler.</param>
        /// <returns>A wrapped box instance.</returns>
        public static IBox InterceptBeforeWith(this IBox box, Action<Envelope> before)
        {
            if (before == null)
            {
                throw new ArgumentNullException(nameof(before));
            }

            return new InterceptorBox(box, before, null);
        }

        /// <summary>
        ///Adds after interception to an <see cref="IBox"/>.
        /// </summary>
        /// <param name="box">An existing box instance.</param>
        /// <param name="after">An after handler.</param>
        /// <returns>A wrapped box instance.</returns>
        public static IBox InterceptAfterWith(this IBox box, Action<Envelope> after)
        {
            if (after == null)
            {
                throw new ArgumentNullException(nameof(after));
            }

            return new InterceptorBox(box, null, after);
        }
    }
}
