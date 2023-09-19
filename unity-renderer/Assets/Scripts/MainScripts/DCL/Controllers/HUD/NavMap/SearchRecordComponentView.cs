using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

public class SearchRecordComponentView : BaseComponentView<SearchRecordComponentModel>, ISearchRecordComponentView
{
    [SerializeField] private TMP_Text recordText;
    [SerializeField] private GameObject historyIcon;
    [SerializeField] private GameObject regularIcon;
    [SerializeField] private Button recordButton;

    public event Action<string> OnSelectedHistoryRecord;
    public event Action<string> OnSelectedRegularRecord;

    public override void Awake()
    {
        base.Awake();

        recordButton.onClick.RemoveAllListeners();
        recordButton.onClick.AddListener(OnSelectedRecord);
    }

    public void OnSelectedRecord()
    {
        if (model.isHistory)
        {
            OnSelectedHistoryRecord?.Invoke(model.recordText);
        }
        else
        {
            OnSelectedRegularRecord?.Invoke(model.recordText);
        }
    }

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
