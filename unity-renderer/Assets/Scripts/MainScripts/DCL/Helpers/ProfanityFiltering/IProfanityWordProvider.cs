using System.Collections.Generic;

namespace DCL.ProfanityFiltering
{
    public interface IProfanityWordProvider
    {
        IEnumerable<string> GetExplicitWords();
        IEnumerable<string> GetNonExplicitWords();
    }
}
