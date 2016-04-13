using System.Text.RegularExpressions;

namespace PostalNET
{
    /// <summary>
    /// Checks if two channel or topic match using wildcards.
    /// </summary>
    public sealed class WildcardChannelTopicMatcher : IChannelTopicMatcher
    {
        public static readonly IChannelTopicMatcher Instance = new WildcardChannelTopicMatcher();

        public bool Matches(string channelOrTopic1, string channelOrTopic2)
        {
            var regex = new Regex("^" + this.Normalize(channelOrTopic1) + "$");

            return regex.IsMatch(channelOrTopic2);
        }

        private string Normalize(string str)
        {
            return str
                .Replace(".", "\\.")
                .Replace(Postal.All, "." + Postal.All);
        }
    }
}
