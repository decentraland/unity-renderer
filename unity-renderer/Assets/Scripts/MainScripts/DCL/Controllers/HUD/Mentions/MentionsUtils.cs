using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DCL.Social.Chat.Mentions
{
    public static class MentionsUtils
    {
        private const string MENTION_URL_PREFIX = "mention://";
        private const string MENTION_PATTERN = "^@[a-zA-Z\\d]{3,15}(#[a-zA-Z\\d]{4})?$";
        private static readonly Regex MENTION_REGEX = new (MENTION_PATTERN);

        public static bool IsAMention(string text)
        {
            var match = MENTION_REGEX.Match(text);
            return match.Success;
        }

        public static string GetUserNameFromMentionLink(string link) =>
            link.Replace(MENTION_URL_PREFIX, string.Empty);

        public static bool IsUserMentionedInText(string userName, string text) =>
            text.ToLower().Contains($"{MENTION_URL_PREFIX}{userName.ToLower()}");

        public static bool TextContainsMention(string text) =>
            text.Contains(MENTION_URL_PREFIX);

        public static List<string> ExtractMentionsFromText(string text)
        {
            List<string> mentionsFound = new List<string>();

            string[] separatedWords = text
                                     .Replace("<noparse>", "")
                                     .Replace("</noparse>", "")
                                     .Replace('\n', ' ')
                                     .Replace('.', ' ')
                                     .Replace(',', ' ')
                                     .Split(' ');

            for (var i = 0; i < separatedWords.Length; i++)
            {
                if (IsAMention(separatedWords[i]))
                    mentionsFound.Add(separatedWords[i]);
            }

            return mentionsFound;
        }
    }
}
