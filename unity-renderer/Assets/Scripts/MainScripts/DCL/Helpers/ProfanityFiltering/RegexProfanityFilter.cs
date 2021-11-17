using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class RegexProfanityFilter
{
    private readonly Regex regex;

    public RegexProfanityFilter(IProfanityWordProvider wordProvider)
    {
        var explicitWords = ToRegex(wordProvider.GetExplicitWords());
        var nonExplicitWords = ToRegex(wordProvider.GetNonExplicitWords());
        regex = new Regex(@$"\b({explicitWords})\b|({nonExplicitWords})", RegexOptions.IgnoreCase);
    }

    public string Filter(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;
        return regex.Replace(message,
            match => new StringBuilder().Append('*', match.Value.Length).ToString());
    }

    private string ToRegex(IEnumerable<string> words) => string.Join("|", words);
}