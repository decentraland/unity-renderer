using DCL.Helpers;
using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

public class SearchRecordComponentView : BaseComponentView<SearchRecordComponentModel>, ISearchRecordComponentView
{
    [SerializeField] internal TMP_Text recordText;
    [SerializeField] internal TMP_Text recordTextNoPlayerCount;
    [SerializeField] internal GameObject historyIcon;
    [SerializeField] private Button recordButton;
    [SerializeField] internal GameObject playerCountParent;
    [SerializeField] internal TMP_Text playerCount;

    public event Action<string> OnSelectedHistoryRecord;
    public event Action<Vector2Int> OnSelectedRegularRecord;

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
            OnSelectedRegularRecord?.Invoke(model.placeCoordinates);
        }
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetRecordText(model.recordText);
        SetIcon(model.isHistory);
        SetPlayerCount(model.playerCount);
        SetCoordinates(model.placeCoordinates);
    }

    public void SetRecordText(string text)
    {
        model.recordText = text;
        recordText.text = text;
        recordTextNoPlayerCount.text = text;
    }

    public void SetIcon(bool isHistory)
    {
        model.isHistory = isHistory;
        historyIcon.SetActive(isHistory);
    }

    public void SetPlayerCount(int count)
    {
        playerCountParent.SetActive(count > 0);
        recordText.gameObject.SetActive(count > 0);
        recordTextNoPlayerCount.gameObject.SetActive(count == 0);
        playerCount.text = count.ToString();
    }

    public void SetCoordinates(Vector2Int coordinates)
    {
        model.placeCoordinates = coordinates;
    }
}
