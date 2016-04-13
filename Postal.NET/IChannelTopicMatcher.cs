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
        /// <param name="channelOrTopic1">A channel or topic.</param>
        /// <param name="channelOrTopic2">Another channel or topic.</param>
        /// <returns>True if the value matches.</returns>
        bool Matches(string channelOrTopic1, string channelOrTopic2);
    }
}
