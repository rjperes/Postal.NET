using System;
using PostalNET;

namespace PostalConventionsNET
{
    public static class ConventionsBoxExtensions
    {
        public static IConventionsBox WithConventions(this IBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return new ConventionsBox(box);
        }

        public static IConventionsBox WithDefaultConventions(this IBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            return new ConventionsBox(box)
                .AddChannelConvention<object>((obj) => obj.GetType().Namespace)
                .AddTopicConvention<object>((obj) => obj.GetType().Name);
        }
    }
}
