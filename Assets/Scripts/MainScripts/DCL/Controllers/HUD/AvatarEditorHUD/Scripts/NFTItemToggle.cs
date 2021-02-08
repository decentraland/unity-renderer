using System;
using UnityEngine;
using UnityEngine.UI;

public class NFTItemToggle : ItemToggle
{
    [SerializeField] internal NFTItemInfo nftItemInfo;
    [SerializeField] internal Button infoButton;
    [SerializeField] internal Button closeInfoButton;
    [SerializeField] internal Button sellButton;

    private static event Action OnHideAllInfos;

    protected override void Awake()
    {
        base.Awake();

        OnHideAllInfos += HideInfo;

        HideInfo();
        infoButton.onClick.AddListener(ToggleInfo);
        closeInfoButton.onClick.AddListener(HideInfo);
        sellButton.onClick.AddListener(CallOnSellClicked);
    }

    public override void Initialize(WearableItem w, bool isSelected, int amount)
    {
        base.Initialize(w, isSelected, amount);
        nftItemInfo.SetModel(NFTItemInfo.Model.FromWearableItem(wearableItem));
    }

    protected override void SetSelection(bool isSelected)
    {
        base.SetSelection(isSelected);
        OnHideAllInfos?.Invoke();
    }

    protected override void OnDestroy()
    {
        OnHideAllInfos -= HideInfo;
        base.OnDestroy();
    }

    private void ShowInfo()
    {
        if (!nftItemInfo.gameObject.activeSelf)
            AudioScriptableObjects.dialogOpen.Play(true);

        OnHideAllInfos?.Invoke();
        nftItemInfo.SetActive(true);
    }

    private void HideInfo()
    {
        if (nftItemInfo.gameObject.activeSelf)
            AudioScriptableObjects.dialogClose.Play(true);

        nftItemInfo.SetActive(false);
    }

    private void ToggleInfo()
    {
        if (nftItemInfo.gameObject.activeSelf)
        {
            HideInfo();
        }
        else
        {
            ShowInfo();
        }
    }
}
