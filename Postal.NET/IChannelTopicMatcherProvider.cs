namespace PostalNET
{
    /// <summary>
    /// Injects a channel and topic matcher.
    /// </summary>
    public interface IChannelTopicMatcherProvider
    {
        /// <summary>
        /// The matcher.
        /// </summary>
        IChannelTopicMatcher Matcher { get; }
    }
}
