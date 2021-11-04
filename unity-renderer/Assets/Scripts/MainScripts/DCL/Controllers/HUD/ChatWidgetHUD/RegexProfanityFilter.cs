using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class RegexProfanityFilter
{
    private readonly Regex regex;

    public RegexProfanityFilter(IProfanityWordProvider wordProvider)
    {
        var words = wordProvider.GetAll();
        var join = string.Join("|", words);
        regex = new Regex(@$"\b({join})\b", RegexOptions.IgnoreCase);
    }

    public string Filter(string message)
    {
        return regex.Replace(message,
            match => new StringBuilder().Append('*', match.Value.Length).ToString());
    }
}