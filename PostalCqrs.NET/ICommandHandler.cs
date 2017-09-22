namespace PostalNET.Cqrs
{
    /// <summary>
    /// The contract for a <see cref="ICommand"/> handler.
    /// </summary>
    /// <typeparam name="T">A command implementation.</typeparam>
    public interface ICommandHandler<T> : IAsyncHandler<T> where T : ICommand
    {
    }
}
