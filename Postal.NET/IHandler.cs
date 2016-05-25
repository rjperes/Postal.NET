namespace PostalNET
{
    /// <summary>
    /// Basic contract for a typed message handler.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    public interface IHandler<T>
    {
        /// <summary>
        /// Processes the received message.
        /// </summary>
        /// <param name="msg">The message.</param>
        void Handle(T msg);
    }
}
