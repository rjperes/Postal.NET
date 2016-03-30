namespace Postal.NET
{
    /// <summary>
    /// An event channel.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// Returns a topic.
        /// </summary>
        /// <param name="topic">The topic name.</param>
        /// <returns>The topic.</returns>
        ITopic Topic(string topic);
    }
}
