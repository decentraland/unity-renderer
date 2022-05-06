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
    private const float SEARCH_DELAY_OFFSET = 1f;
    public event Action<List<Dictionary<string, List<CatalogItem>>>> OnFilterChange;
    public event Action OnFilterRemove;

    internal List<Dictionary<string, List<CatalogItem>>> filterObjects = new List<Dictionary<string, List<CatalogItem>>>();

    private IBIWSearchBarView view;

    private bool isSmartItemFilterActive = false;
    private bool isFilterActive = false;

    private string currentSearchFilter;
    private Coroutine searchApplyCoroutine;

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
        if (view == null)
            return;
        
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
        currentSearchFilter = currentSearchInput;

        isFilterActive = true;

        FilterAssets(currentSearchInput);
        OnFilterChange?.Invoke(filterObjects);
        if (searchApplyCoroutine != null)
            CoroutineStarter.Stop(searchApplyCoroutine);
        searchApplyCoroutine = CoroutineStarter.Start(ReportSearchAnalytic());
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
        StringComparison comparison = StringComparison.OrdinalIgnoreCase;
        filterObjects.Clear();
        foreach (CatalogItem catalogItem in DataStore.i.builderInWorld.catalogItemDict.GetValues())
        {
            if (!MatchtFilter(catalogItem, nameToFilter, comparison))
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

    private bool MatchtFilter(CatalogItem catalogItem, string nameToFilter, StringComparison comparison)
    {
        string nameToFilterToLower = nameToFilter.ToLower();

        // NOTE: Due to an Unity known issue, the use of 'StringComparison.OrdinalIgnoreCase' in WebGL is case sensitive when shouldn't be.
        // Absurd as it may seem, Unity says it is working in this way "by design", so it seems they're not going to fix it.
        // A work-around is to use '.ToLower()' in both strings.
        // More info: https://issuetracker.unity3d.com/issues/webgl-build-system-dot-stringcomparison-dot-ordinalignorecase-is-case-sensitive
        if (catalogItem.category.ToLower().IndexOf(nameToFilterToLower, comparison) != -1 ||
            catalogItem.name.ToLower().IndexOf(nameToFilterToLower, comparison) != -1)
            return true;

        foreach (string tag in catalogItem.tags)
        {
            if (tag.ToLower().IndexOf(nameToFilterToLower, comparison) != -1)
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

    IEnumerator ReportSearchAnalytic()
    {
        yield return new WaitForSecondsRealtime(SEARCH_DELAY_OFFSET);
        BIWAnalytics.CatalogItemSearched(currentSearchFilter, filterObjects.Count);
    }
}