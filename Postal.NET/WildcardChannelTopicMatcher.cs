using System;
using System.Text.RegularExpressions;

namespace PostalNET
{
    /// <summary>
    /// Checks if two channel or topic match using wildcards.
    /// </summary>
    public sealed class WildcardChannelTopicMatcher : IChannelTopicMatcher
    {
        public static readonly IChannelTopicMatcher Instance = new WildcardChannelTopicMatcher();

        public bool Matches(string subscribedChannelOrTopic, string publishedChannelOrTopic)
        {
            if (string.IsNullOrWhiteSpace(subscribedChannelOrTopic) == true)
            {
                throw new ArgumentNullException("subscribedChannelOrTopic");
            }

            if (string.IsNullOrWhiteSpace(publishedChannelOrTopic) == true)
            {
                throw new ArgumentNullException("publishedChannelOrTopic");
            }

            if (publishedChannelOrTopic.Contains(Postal.All) == true)
            {
                throw new ArgumentException("The published channel or topic cannot have wildcards", "publishedChannelOrTopic");
            }

            var regex = new Regex("^" + this.Normalize(subscribedChannelOrTopic) + "$");

            return regex.IsMatch(publishedChannelOrTopic);
        }

        private string Normalize(string str)
        {
            return str
                .Replace(".", "\\.")
                .Replace(Postal.All, "." + Postal.All);
        }
    }
}
