using System;

public interface ISearchRecordComponentView
{
    event Action<string> OnSelectedHistoryRecord;
    event Action<string> OnSelectedRegularRecord;

    void SetRecordText(string text);
    void SetIcon(bool isHistory);
}
