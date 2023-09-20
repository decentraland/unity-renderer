using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

public class SearchRecordComponentView : BaseComponentView<SearchRecordComponentModel>, ISearchRecordComponentView
{
    [SerializeField] private TMP_Text recordText;
    [SerializeField] private GameObject historyIcon;
    [SerializeField] private Button recordButton;
    [SerializeField] private GameObject playerCountParent;
    [SerializeField] private TMP_Text playerCount;

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
        SetPlayerCount(model.playerCount);
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
    }

    public void SetPlayerCount(int count)
    {
        playerCountParent.SetActive(count > 0);
        playerCount.text = count.ToString();
    }
}
