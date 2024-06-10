using System;
using System.Collections.Generic;

namespace DCL
{
    public class StringIgnoreCaseEqualityComparer : IEqualityComparer<string>
    {
        public static StringIgnoreCaseEqualityComparer Default { get; } = new ();

        public bool Equals(string x, string y) =>
            string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode(string obj) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
    }
}
