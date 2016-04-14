namespace PostalNET
{
    public interface IChannelTopicMatcherProvider
    {
        IChannelTopicMatcher Matcher { get; }
    }
}
