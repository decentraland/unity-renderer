using System;
using System.Collections.Generic;

public interface IViewAllComponentView
{
    event Action OnBackFromViewAll;
    event Action<string, int, int> OnRequestCollectibleElements;

    void Initialize(string sectionName);
    void SetTotalElements(int totalElements);
    void SetSectionQuantity(int totalCount);
    void SetVisible(bool isVisible);
    void ShowNftIcons(List<NFTIconComponentModel> models);
}
