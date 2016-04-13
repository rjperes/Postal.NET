using PostalNET;
using System;

namespace PostalWhenNET
{
    public static class BoxExtensions
    {
        public static ICompositionBox When(this IBox box, string channel, string topic, Func<Envelope, bool> condition = null)
        {
            return new CompositionBox(box).And(channel, topic, condition);
        }
    }
}
