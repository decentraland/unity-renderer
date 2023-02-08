using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DCL.ProfanityFiltering
{
    public class RegexProfanityFilter : IProfanityFilter
    {
        private readonly IProfanityWordProvider wordProvider;
        private Regex regex;

        public RegexProfanityFilter(IProfanityWordProvider wordProvider)
        {
            this.wordProvider = wordProvider;
        }

        public void Initialize()
        {
            string explicitWords = ToRegex(wordProvider.GetExplicitWords());
            string nonExplicitWords = ToRegex(wordProvider.GetNonExplicitWords());
            regex = new Regex(@$"\b({explicitWords})\b|({nonExplicitWords})", RegexOptions.IgnoreCase);
        }

        public void Dispose()
        {
        }

        public async UniTask<string> Filter(string message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(message)) return message;
            return regex.Replace(message,
                match => new StringBuilder().Append('*', match.Value.Length).ToString());
        }

        private string ToRegex(IEnumerable<string> words) => string.Join("|", words);
    }
}
