using System.Collections.Generic;

public interface IProfanityWordProvider
{
    IEnumerable<string> GetExplicitWords();
    IEnumerable<string> GetNonExplicitWords();
}