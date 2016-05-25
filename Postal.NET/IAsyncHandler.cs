using System.Threading.Tasks;

namespace PostalNET
{
    // <summary>
    /// Basic contract for an asynchronous typed message handler.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    public interface IAsyncHandler<T>
    {
        /// <summary>
        /// Processes the received message asynchronously.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>A task.</returns>
        Task HandleAsync(T msg);
    }
}