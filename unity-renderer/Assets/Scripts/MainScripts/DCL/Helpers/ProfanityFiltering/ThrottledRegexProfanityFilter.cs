﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class ThrottledRegexProfanityFilter : IProfanityFilter
{
    private readonly List<Regex> regexSteps = new List<Regex>();

    public ThrottledRegexProfanityFilter(IProfanityWordProvider wordProvider, int partitionSize = 1)
    {
        List<string> explicitWords = wordProvider.GetExplicitWords().ToList();
        List<string> nonExplicitWords = wordProvider.GetNonExplicitWords().ToList();

        var explicitWordsChunks = ToChunks(explicitWords, partitionSize);
        var nonExplicitWordsChunks = ToChunks(nonExplicitWords, partitionSize);

        for (int i = 0; i < explicitWordsChunks.Count; i++)
        {
            var explicitWordsRegex = ToRegex(explicitWordsChunks[i]);
            var regex = new Regex(@$"\b({explicitWordsRegex})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            regexSteps.Add(regex);
        }

        for (int i = 0; i < nonExplicitWordsChunks.Count; i++)
        {
            var nonExplicitWordsRegex = ToRegex(nonExplicitWordsChunks[i]);
            var regex = new Regex(@$"\\b|({nonExplicitWordsRegex})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            regexSteps.Add(regex);
        }
    }

    public async UniTask<string> Filter(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        foreach (Regex regexStep in regexSteps)
        {
            await CheckTimerAndSkipFrame(stopwatch);
            message = regexStep.Replace(message, match => new StringBuilder().Append('*', match.Value.Length).ToString());
        }

        return message;
    }

    private async Task CheckTimerAndSkipFrame(Stopwatch stopwatch)
    {
        if (stopwatch.ElapsedMilliseconds > 1)
        {
            await UniTask.WaitForEndOfFrame();
            stopwatch.Restart();
        }
    }

    private string ToRegex(IEnumerable<string> words) => string.Join("|", words);

    private List<List<T>> ToChunks<T>(List<T> source, int chunkSize)
    {
        return source
               .Select((value, index) => (index, value))
               .GroupBy(x => x.index / chunkSize)
               .Select(x => x.Select(v => v.value).ToList())
               .ToList();
    }
}