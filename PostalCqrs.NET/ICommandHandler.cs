using PostalNET;

namespace PostalCqrsNET
{
    public interface ICommandHandler<T> : IAsyncHandler<T> where T : ICommand
    {
    }
}
