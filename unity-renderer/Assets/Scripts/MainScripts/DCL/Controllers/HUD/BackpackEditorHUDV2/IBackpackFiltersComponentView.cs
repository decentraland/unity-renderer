using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IBackpackFiltersComponentView
    {
        event Action<bool> OnOnlyCollectiblesChanged;
        event Action<HashSet<string>> OnCollectionChanged;
        event Action<string> OnSortByChanged;
        event Action<string> OnSearchTextChanged;

        void Dispose();
    }
}
