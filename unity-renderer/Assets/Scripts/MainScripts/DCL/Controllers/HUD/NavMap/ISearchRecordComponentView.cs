using System;
using UnityEngine;

public interface ISearchRecordComponentView
{
    event Action<string> OnSelectedHistoryRecord;
    event Action<Vector2Int> OnSelectedRegularRecord;

    void SetRecordText(string text);
    void SetIcon(bool isHistory);
    void SetPlayerCount(int count);
    void SetCoordinates(Vector2Int coordinates);
}
