using System;
using UnityEngine;

public class NftPageView : BaseComponentView
{

    public event Action<string, string> OnClickBuyNft;
    public event Action OnFocusAnyNtf;

    [SerializeField] private NFTIconComponentView[] nftElements;

    public bool IsNftInfoActived { get; set; } = false;

    public void SetPageElementsContent(NFTIconComponentModel[] nftModels)
    {
        for (int i = 0; i < nftModels.Length; i++)
        {
            if (nftModels[i] != null)
            {
                nftElements[i].gameObject.SetActive(true);
                nftElements[i].Configure(nftModels[i]);
                RegisterNftElementActions(nftElements[i], i);
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

    private void RegisterNftElementActions(NFTIconComponentView nftElement, int idIndex)
    {
        nftElement.onMarketplaceButtonClick.RemoveAllListeners();
        nftElement.onDetailInfoButtonClick.RemoveAllListeners();
        nftElement.onMarketplaceButtonClick.AddListener(() => ClickOnBuyWearable(nftElement.model.nftInfo));
        nftElement.onDetailInfoButtonClick.AddListener(() => ClickOnDetailInfo(idIndex, showInLeftSide: idIndex == 3));
        nftElement.onFocused -= FocusNftItem;
        nftElement.onFocused += FocusNftItem;
    }

    private void ClickOnBuyWearable(NftInfo idsAndCategory)
    {
        OnClickBuyNft?.Invoke(idsAndCategory.Id, idsAndCategory.Category);
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
