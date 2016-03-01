namespace Postal.NET
{
    internal sealed class Channel : IChannel
    {
        private readonly IBox box;
        private readonly string channel;

        public Channel(IBox box, string channel)
        {
            this.box = box;
            this.channel = channel;
        }

        public ITopic Topic(string topic)
        {
            return new Topic(this.box, this.channel, topic);
        }
    }
}
