using PostalNET;
using System;

namespace PostalWhenNET
{
    public interface ICompositionBox
    {
        ICompositionBox And(string channel, string topic, Func<Envelope, bool> condition = null);
        ICompositionBox InTime(TimeSpan time);
        IDisposable Subscribe(Action<Envelope> subscriber);
    }
}
