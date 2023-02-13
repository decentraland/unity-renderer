
using System;

public interface IViewAllComponentView
{
    event Action OnBackFromViewAll;
    event Action<int, int> OnRequestCollectibleElements;

    void Initialize(int totalCollectiblesElements);
    void SetSectionName(string sectionNameText);
    void SetSectionQuantity(int totalCount);
    void SetVisible(bool isVisible);
}
