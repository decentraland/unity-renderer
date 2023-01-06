using System;
using UnityEngine;

public class NftPageView : BaseComponentView
{

    public event Action<string, string> OnClickBuyNft;
    public event Action OnFocusAnyNtf;

    [SerializeField] private NFTIconComponentView[] nftElements;
    private (string, string)[] nftIds = new (string, string)[4];

    public bool IsNftInfoActived { get; set; } = false;

    public void SetPageElementsContent(NFTIconComponentModel[] nftModels, (string, string)[] ids)
    {
        nftIds = ids;
        for (int i = 0; i < nftModels.Length; i++)
        {
            if (nftModels[i] != null)
            {
                nftElements[i].gameObject.SetActive(true);
                nftElements[i].Configure(nftModels[i]);
                nftElements[i].onMarketplaceButtonClick.RemoveAllListeners();
                nftElements[i].onDetailInfoButtonClick.RemoveAllListeners();
                nftElements[i].onFocused -= FocusNftItem;
                int idIndex = i;
                nftElements[i].onMarketplaceButtonClick.AddListener(() => ClickOnBuyWearable(idIndex));
                nftElements[i].onDetailInfoButtonClick.AddListener(() => ClickOnDetailInfo(idIndex, showInLeftSide: idIndex == 3));
                nftElements[i].onFocused += FocusNftItem;
            }
            else
            {
                nftElements[i].gameObject.SetActive(false);
            }
        }
    }

    public void ConfigureNFTItemInfo(NFTItemInfo nftItemInfoModal, WearableItem[] wearableItems, bool showCategoryInfo)
    {
        for (int i = 0; i < nftElements.Length; i++)
        {
            if (wearableItems[i] != null)
                nftElements[i].ConfigureNFTItemInfo(nftItemInfoModal, wearableItems[i], showCategoryInfo);
        }
    }

    public void CloseAllNFTItemInfos()
    {
        for (int i = 0; i < nftElements.Length; i++)
            nftElements[i].SetNFTItemInfoActive(false);
    }

    private void ClickOnBuyWearable(int index)
    {
        OnClickBuyNft?.Invoke(nftIds[index].Item1, nftIds[index].Item2);
    }

    private void ClickOnDetailInfo(int index, bool showInLeftSide)
    {
        if (IsNftInfoActived)
            nftElements[index].SetNFTItemInfoActive(true, showInLeftSide);
    }

    private void FocusNftItem(bool isFocused)
    {
        if (!isFocused)
            return;

        OnFocusAnyNtf?.Invoke();
    }

    public override void RefreshControl()
    {
    }
}
