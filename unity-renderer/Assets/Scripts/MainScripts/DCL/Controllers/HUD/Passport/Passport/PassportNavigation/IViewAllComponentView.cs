using System;
using System.Collections.Generic;

public interface IViewAllComponentView
{
    event Action OnBackFromViewAll;
    event Action<PassportSection, int, int> OnRequestCollectibleElements;
    event Action<NftInfo> OnClickBuyNft;

    void Initialize(PassportSection sectionName);
    void SetTotalElements(int totalElements);
    void SetVisible(bool isVisible);
    void ShowNftIcons(List<(NFTIconComponentModel model, WearableItem wearable)> iconsWithWearables);
    void SetLoadingActive(bool isLoading);
    void CloseAllNftItemInfos();
}
