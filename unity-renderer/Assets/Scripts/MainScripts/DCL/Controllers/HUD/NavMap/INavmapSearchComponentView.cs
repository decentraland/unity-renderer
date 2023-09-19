using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;

public interface INavmapSearchComponentView
{
    event Action<bool> OnSelectedSearchBar;
    event Action<string> OnSearchedText;

    void SetHistoryRecords(string[] previousSearches);
    void SetSearchResultRecords(IReadOnlyList<IHotScenesController.PlaceInfo> places);
    void ClearResults();
}
