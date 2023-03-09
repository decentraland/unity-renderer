using System.Text.RegularExpressions;

namespace DCL.Social.Chat.Mentions
{
    public static class MentionsUtils
    {
        private const string MENTION_URL_PREFIX = "mention://";
        private const string MENTION_PATTERN = "^@[a-zA-Z\\d]{3,15}(#[a-zA-Z\\d]{4})?$";
        private static readonly Regex MENTION_REGEX = new (MENTION_PATTERN);

        public static bool IsAMention(string text, string link)
        {
            var match = MENTION_REGEX.Match(text);
            return match.Success && link.StartsWith(MENTION_URL_PREFIX);
        }

        public static string GetUserIdFromMentionLink(string link) =>
            link.Replace(MENTION_URL_PREFIX, string.Empty).ToLower();

        public static bool IsUserMentionedInText(string userId, string text) =>
            text.ToLower().Contains($"{MENTION_URL_PREFIX}{userId.ToLower()}");

        public static bool TextContainsMention(string text) =>
            text.ToLower().Contains(MENTION_URL_PREFIX);
    }
}
