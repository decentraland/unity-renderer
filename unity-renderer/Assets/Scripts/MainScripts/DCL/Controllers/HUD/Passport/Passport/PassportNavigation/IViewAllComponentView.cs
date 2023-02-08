
using System;

public interface IViewAllComponentView
{
    event Action OnBackFromViewAll;
    void SetSectionName(string sectionNameText);
    void SetVisible(bool isVisible);
}
