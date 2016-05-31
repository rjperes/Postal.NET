using PostalNET;

namespace PostalCqrsNET
{
    public interface IQueryHandler<T> : IHandler<T> where T : IQuery
    {
    }
}
