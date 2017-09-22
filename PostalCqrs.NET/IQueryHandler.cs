namespace PostalNET.Cqrs
{
    /// <summary>
    /// The contract for a <see cref="IQuery"/> handler.
    /// </summary>
    /// <typeparam name="T">A query implementation.</typeparam>
    public interface IQueryHandler<T> : IHandler<T> where T : IQuery
    {
    }
}
