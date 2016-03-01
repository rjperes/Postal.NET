namespace Postal.NET
{
    public interface IChannel
    {
        ITopic Topic(string topic);
    }
}
