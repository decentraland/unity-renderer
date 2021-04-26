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

        return item.keywords.Any(keyword => !string.IsNullOrEmpty(keyword) && keyword.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public static void Sort<T>(string sortType, List<T> input, bool descendingOrder) where T : ISortable<T>
    {
        input.Sort((a, b) => a.Compare(sortType, descendingOrder, b));
    }

    public static List<T> Filter<T>(List<T> input, Predicate<T> match)
    {
        return input.FindAll(match);
    }
}
