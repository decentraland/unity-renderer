using System.Collections.Generic;

public class ChannelUtils
{
    public static List<string> ExtractChannelPatternsFromText(string text)
    {
        List<string> channelsFound = new List<string>();
        
        string[] separatedWords = text
            .Replace("<noparse>", "")
            .Replace("</noparse>", "")
            .Split(' ');

        for (int i = 0; i < separatedWords.Length; i++)
        {
            if ((separatedWords[i].StartsWith("#") && separatedWords[i].Length > 1) || separatedWords[i].ToLower() == "~nearby")
                channelsFound.Add(separatedWords[i]);
        }

        return channelsFound;
    }
}