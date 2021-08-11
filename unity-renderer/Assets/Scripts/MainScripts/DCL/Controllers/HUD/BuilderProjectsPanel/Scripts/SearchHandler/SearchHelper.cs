using System;
using System.Collections.Generic;
using System.Linq;

public static class SearchHelper
{
    public static List<T> Search<T>(string text, List<T> input) where T : ISearchable
    {
        if (string.IsNullOrEmpty(text))
            return new List<T>(input);

        return input.FindAll(item => SearchMatchItem(text, item));
    }

    public static bool SearchMatchItem<T>(string text, T item) where T : ISearchable
    {
        if (string.IsNullOrEmpty(text))
            return true;

        // NOTE: Due to an Unity known issue, the use of 'StringComparison.OrdinalIgnoreCase' in WebGL is case sensitive when shouldn't be.
        // Absurd as it may seem, Unity says it is working in this way "by design", so it seems they're not going to fix it.
        // A work-around is to use '.ToLower()' in both strings.
        // More info: https://issuetracker.unity3d.com/issues/webgl-build-system-dot-stringcomparison-dot-ordinalignorecase-is-case-sensitive
        return item.keywords.Any(keyword => !string.IsNullOrEmpty(keyword) && keyword.ToLower().IndexOf(text.ToLower(), StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public static void Sort<T>(string sortType, List<T> input, bool descendingOrder) where T : ISortable<T> { input.Sort((a, b) => a.Compare(sortType, descendingOrder, b)); }

    public static List<T> Filter<T>(List<T> input, Predicate<T> match) { return input.FindAll(match); }
}