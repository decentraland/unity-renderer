using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface INavmapSearchComponentView
{
    event Action<bool> OnSelectedSearchBar;
    event Action<string> OnSearchedText;
    event Action<Vector2Int> OnSelectedSearchRecord;

    void SetHistoryRecords(string[] previousSearches);
    void SetSearchResultRecords(IReadOnlyList<IHotScenesController.PlaceInfo> places);
    void ClearResults();
}
