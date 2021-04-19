using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public interface IBIWSearchBarController
{
    event Action<List<Dictionary<string, List<CatalogItem>>>> OnFilterChange;
    event Action OnFilterRemove;
    bool IsFilterActive();
    void Dispose();
    List<Dictionary<string, List<CatalogItem>>> FilterAssets(string nameToFilter);
}

public class BIWSearchBarController : IBIWSearchBarController
{
    public event Action<List<Dictionary<string, List<CatalogItem>>>> OnFilterChange;
    public event Action OnFilterRemove;

    internal List<Dictionary<string, List<CatalogItem>>> filterObjects = new List<Dictionary<string, List<CatalogItem>>>();

    private IBIWSearchBarView view;

    private bool isSmartItemFilterActive = false;
    private bool isFilterActive = false;

    public void Initialize(ISceneCatalogView sceneCatalogView)
    {
        this.view = sceneCatalogView.searchBarView;
        if (this.view.searchInput != null)
            this.view.searchInput.onValueChanged.AddListener(OnSearchInputChanged);

        if (this.view.smartItemBtn != null)
            this.view.smartItemBtn.onClick.AddListener(ChangeSmartItemFilter);
    }

    public void Dispose()
    {
        if (view.searchInput != null)
            view.searchInput.onValueChanged.RemoveListener(OnSearchInputChanged);

        if (view.smartItemBtn != null)
            view.smartItemBtn.onClick.RemoveListener(ChangeSmartItemFilter);
    }

    public bool IsFilterActive() { return isFilterActive; }

    public void OnSearchInputChanged(string currentSearchInput)
    {
        if (string.IsNullOrEmpty(currentSearchInput))
        {
            OnFilterRemove?.Invoke();
            return;
        }

        if (currentSearchInput.Length <= 1)
            return;
        isFilterActive = true;
        FilterAssets(currentSearchInput);
        OnFilterChange?.Invoke(filterObjects);
    }

    public void ReleaseFilters()
    {
        isSmartItemFilterActive = false;
        filterObjects.Clear();
        view.SetSmartItemPressStatus(false);
        view.SetEmptyFilter();

        isFilterActive = false;
    }

    public void ChangeSmartItemFilter()
    {
        isSmartItemFilterActive = !isSmartItemFilterActive;
        view.SetSmartItemPressStatus(isSmartItemFilterActive);

        if (isSmartItemFilterActive)
            FilterSmartItem();
        else
        {
            isFilterActive = false;
            OnFilterRemove?.Invoke();
        }

    }

    public void FilterSmartItem()
    {
        filterObjects.Clear();
        foreach (CatalogItem catalogItem in DataStore.i.builderInWorld.catalogItemDict.GetValues())
        {
            if (!catalogItem.IsSmartItem())
                continue;

            bool foundCategory = false;
            foreach (Dictionary<string, List<CatalogItem>> groupedSceneObjects in filterObjects)
            {
                if (groupedSceneObjects.ContainsKey(catalogItem.category))
                {
                    foundCategory = true;
                    if (!groupedSceneObjects[catalogItem.category].Contains(catalogItem))
                        groupedSceneObjects[catalogItem.category].Add(catalogItem);
                }
            }
            if (!foundCategory)
            {
                AddNewSceneObjectCategoryToFilter(catalogItem);
            }
        }

        OnFilterChange?.Invoke(filterObjects);
    }

    public List<Dictionary<string, List<CatalogItem>>> FilterAssets(string nameToFilter)
    {
        filterObjects.Clear();
        foreach (CatalogItem catalogItem in DataStore.i.builderInWorld.catalogItemDict.GetValues())
        {
            if (!MatchtFilter(catalogItem, nameToFilter))
                continue;

            bool foundCategory = false;
            foreach (Dictionary<string, List<CatalogItem>> groupedSceneObjects in filterObjects)
            {
                if (groupedSceneObjects.ContainsKey(catalogItem.category))
                {
                    foundCategory = true;
                    if (!groupedSceneObjects[catalogItem.category].Contains(catalogItem))
                        groupedSceneObjects[catalogItem.category].Add(catalogItem);
                }
            }
            if (!foundCategory)
            {
                AddNewSceneObjectCategoryToFilter(catalogItem);
            }
        }
        return filterObjects;
    }

    private bool MatchtFilter(CatalogItem catalogItem, string nameToFilter)
    {
        if (catalogItem.category.IndexOf(nameToFilter, StringComparison.OrdinalIgnoreCase) >= 0  ||
            catalogItem.name.IndexOf(nameToFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            return  true;

        foreach (string tag in catalogItem.tags)
        {
            if (tag.IndexOf(nameToFilter, StringComparison.OrdinalIgnoreCase) >= 0 )
            {
                return true;
            }
        }
        return false;
    }

    internal void AddNewSceneObjectCategoryToFilter(CatalogItem catalogItem)
    {
        Dictionary<string, List<CatalogItem>> groupedCatalogItems = new Dictionary<string, List<CatalogItem>>();
        groupedCatalogItems.Add(catalogItem.category, new List<CatalogItem>() { catalogItem });
        filterObjects.Add(groupedCatalogItems);
    }
}