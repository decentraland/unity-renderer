using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

public class RegexProfanityFilter : IProfanityFilter
{
    private readonly Regex regex;

    public RegexProfanityFilter(IProfanityWordProvider wordProvider)
    {
        var explicitWords = ToRegex(wordProvider.GetExplicitWords());
        var nonExplicitWords = ToRegex(wordProvider.GetNonExplicitWords());
        regex = new Regex(@$"\b({explicitWords})\b|({nonExplicitWords})", RegexOptions.IgnoreCase);
    }

    public async UniTask<string> Filter(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;
        return regex.Replace(message,
            match => new StringBuilder().Append('*', match.Value.Length).ToString());
    }

    private string ToRegex(IEnumerable<string> words) => string.Join("|", words);
}