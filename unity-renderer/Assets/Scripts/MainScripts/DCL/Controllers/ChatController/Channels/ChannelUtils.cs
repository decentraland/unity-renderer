using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DCL.Chat.Channels
{
    public static class ChannelUtils
    {
        private const string CHANNEL_MATCH_REGEX = "^#[a-zA-Z0-9-]{3,20}$";
        private const string NEAR_BY_CHANNEL = "~nearby";
        private static readonly Regex filter = new (CHANNEL_MATCH_REGEX);

        public static List<string> ExtractChannelIdsFromText(string text)
        {
            List<string> channelsFound = new List<string>();

            string[] separatedWords = text
                                     .Replace("<noparse>", "")
                                     .Replace("</noparse>", "")
                                     .Replace('\n', ' ')
                                     .Replace('.', ' ')
                                     .Replace(',', ' ')
                                     .Split(' ');

            for (int i = 0; i < separatedWords.Length; i++)
            {
                if (IsAChannel(separatedWords[i]))
                    channelsFound.Add(separatedWords[i]);
            }

            return channelsFound;
        }

        public static bool IsAChannel(string text)
        {
            var match = filter.Match(text);
            return match.Success || text.ToLower() == NEAR_BY_CHANNEL;
        }
    }
}
