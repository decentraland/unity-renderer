using System.Collections.Generic;

public class ProfanityWordProviderInMemory : IProfanityWordProvider
{
    private readonly List<string> words = new List<string>();

    public void Add(string word) => words.Add(word);

    public IEnumerable<string> GetAll() => words;
}