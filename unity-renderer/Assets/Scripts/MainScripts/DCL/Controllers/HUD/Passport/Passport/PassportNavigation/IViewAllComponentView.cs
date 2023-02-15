using System;
using System.Collections.Generic;

public interface IViewAllComponentView
{
    event Action OnBackFromViewAll;
    event Action<string, int, int> OnRequestCollectibleElements;

    void Initialize(int totalCollectiblesElements);
    void SetSectionName(string sectionNameText);
    void SetSectionQuantity(int totalCount);
    void SetVisible(bool isVisible);
    void ShowNftIcons(List<NFTIconComponentModel> models);
}
