using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DCL.Social.Chat.Mentions
{
    public static class MentionsUtils
    {
        private const string MENTION_PATTERN = @"\B@[a-zA-Z\d]{3,15}(#[a-zA-Z\d]{4})?";
        private static readonly Regex MENTION_REGEX = new (MENTION_PATTERN);

        public static bool IsAMention(string text)
        {
            var match = MENTION_REGEX.Match(text);
            return match.Success;
        }

        public static bool IsUserMentionedInText(string userName, string text) =>
            text.Contains($"@{userName}", StringComparison.OrdinalIgnoreCase);

        public static bool TextContainsMention(string text)
        {
            Match match = MENTION_REGEX.Match(text);
            return match.Success;
        }

        public static IEnumerable<string> GetAllMentions(string input)
        {
            MatchCollection matches = MENTION_REGEX.Matches(input);
            return matches.Select(match => match.Value);
        }

        public static string ReplaceMentionPattern(string input, Func<string, string> replacementCallback)
        {
            return MENTION_REGEX.Replace(input, match => replacementCallback?.Invoke(match.Value));
        }
    }
}
