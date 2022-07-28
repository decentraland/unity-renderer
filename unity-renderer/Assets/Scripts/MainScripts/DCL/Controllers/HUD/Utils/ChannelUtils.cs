using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ChannelUtils
{
    public const string CHANNEL_MATCH_REGEX = "^#[a-zA-Z0-9-]{3,20}$";

    public static List<string> ExtractChannelPatternsFromText(string text)
    {
        Regex filter = new Regex(CHANNEL_MATCH_REGEX);
        List<string> channelsFound = new List<string>();
        
        string[] separatedWords = text
            .Replace("<noparse>", "")
            .Replace("</noparse>", "")
            .Replace('\n', ' ')
            .Split(' ');

        for (int i = 0; i < separatedWords.Length; i++)
        {
            var match = filter.Match(separatedWords[i]);

            if (match.Success || separatedWords[i].ToLower() == "~nearby")
                channelsFound.Add(separatedWords[i]);
        }

        return channelsFound;
    }
}