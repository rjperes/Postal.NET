using System;

namespace PostalNET.When
{
    /// <summary>
    /// The contract for a composable <see cref="IBox"/>.
    /// </summary>
    public interface ICompositionBox
    {
        ICompositionBox And(string channel, string topic, Func<Envelope, bool> condition = null);
        ICompositionBox InTime(TimeSpan time);
        IDisposable Subscribe(Action<Envelope> subscriber);
    }
}
