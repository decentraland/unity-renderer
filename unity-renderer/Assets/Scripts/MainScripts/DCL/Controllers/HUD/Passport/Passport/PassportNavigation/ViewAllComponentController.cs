using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewAllComponentController : IDisposable
{
    public event Action OnBackFromViewAll;

    private IViewAllComponentView view;

    public ViewAllComponentController(IViewAllComponentView view)
    {
        this.view = view;
        view.OnBackFromViewAll += BackFromViewAll;
        view.OnRequestCollectibleElements += RequestCollectibleElements;
    }

    private void RequestCollectibleElements(int pageNumber, int pageSize)
    {
        //TODO: Wire request to new refactored implementation
    }

    public void OpenViewAllSection(string sectionName)
    {
        view.Initialize(300);
        view.SetSectionName(sectionName);
    }

    public void SetViewAllVisibility(bool isVisible)
    {
        view.SetVisible(isVisible);
    }

    private void BackFromViewAll()
    {
        OnBackFromViewAll?.Invoke();
        SetViewAllVisibility(false);
    }

    public void Dispose()
    {
        view.OnBackFromViewAll -= BackFromViewAll;
    }
}
