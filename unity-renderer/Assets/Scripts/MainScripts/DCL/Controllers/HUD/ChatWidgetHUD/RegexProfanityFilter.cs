using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class RegexProfanityFilter
{
    private Regex regex;

    public RegexProfanityFilter()
    {
        var words = new[]
        {
            "shit", "ass", "bitch"
        };
        var join = string.Join("|", words.Select(word => $"({word}*)"));
        regex = new Regex(join, RegexOptions.IgnoreCase);
    }

    public string Filter(string message)
    {
        return regex.Replace(message,
            match => new StringBuilder().Append('*', match.Value.Length).ToString());
    }
}