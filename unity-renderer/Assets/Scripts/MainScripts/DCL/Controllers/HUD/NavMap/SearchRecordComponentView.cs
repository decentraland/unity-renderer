using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;

public class SearchRecordComponentView : BaseComponentView<SearchRecordComponentModel>, ISearchRecordComponentView
{
    [SerializeField] private TMP_Text recordText;
    [SerializeField] private GameObject historyIcon;
    [SerializeField] private GameObject regularIcon;

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetRecordText(model.recordText);
        SetIcon(model.isHistory);
    }

    public void SetRecordText(string text)
    {
        model.recordText = text;
        recordText.text = text;
    }

    public void SetIcon(bool isHistory)
    {
        model.isHistory = isHistory;
        historyIcon.SetActive(isHistory);
        regularIcon.SetActive(!isHistory);
    }
}
