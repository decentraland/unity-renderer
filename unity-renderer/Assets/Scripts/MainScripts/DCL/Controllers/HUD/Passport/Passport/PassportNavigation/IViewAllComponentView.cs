
using System;

public interface IViewAllComponentView
{
    event Action OnBackFromViewAll;
    event Action<int, int> OnRequestCollectibleElements;

    void Initialize(int totalCollectiblesElements);
    void SetSectionName(string sectionNameText);
    void SetVisible(bool isVisible);
}
