namespace PostalNET
{
    /// <summary>
    /// How to match channels and topics.
    /// </summary>
    public interface IChannelTopicMatcher
    {
        /// <summary>
        /// Checks if two channel or topic names match.
        /// </summary>
        /// <param name="subscribedChannelOrTopic">A channel or topic.</param>
        /// <param name="publishedChannelOrTopic">Another channel or topic.</param>
        /// <returns>True if the value matches.</returns>
        bool Matches(string subscribedChannelOrTopic, string publishedChannelOrTopic);
    }
}
