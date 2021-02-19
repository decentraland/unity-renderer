
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SceneObjectCatalogController : MonoBehaviour 
{
    public event System.Action<string> OnResultReceived;
    public event System.Action<CatalogItem> OnCatalogItemSelected;
    public event System.Action<CatalogItem, CatalogItemAdapter> OnCatalogItemFavorite;

    [Header("Prefab References")]
    public TextMeshProUGUI catalogTitleTxt;
    public CatalogAssetPackListView catalogAssetPackListView;
    public CatalogGroupListView catalogGroupListView;
    public TMP_InputField searchInputField;
    public FavoritesController favoritesController;
    public QuickBarView quickBarView;
    public Toggle categoryToggle;
    public Toggle favoritesToggle;
    public Toggle assetPackToggle;

    [Header("Catalog RectTransforms")]
    public RectTransform panelRT;
    public RectTransform headerRT;
    public RectTransform searchBarRT;
    public RectTransform assetPackRT;
    public RectTransform categoryRT;

    [Header("MinSize Catalog RectTransforms")]

    public RectTransform panelMinSizeRT;
    public RectTransform headerMinSizeRT;
    public RectTransform searchBarMinSizeRT;
    public RectTransform assetPackMinSizeRT;

    [Header("MaxSize Catalog RectTransforms")]

    public RectTransform panelMaxSizeRT;
    public RectTransform headerMaxSizeRT;
    public RectTransform searchBarMaxSizeRT;
    public RectTransform assetPackMaxSizeRT;

    private List<Dictionary<string, List<CatalogItem>>> filterObjects = new List<Dictionary<string, List<CatalogItem>>>();

    private string lastFilterName = "";
    private bool isShowingAssetPacks = false;
    private bool isFavoriteFilterActive = false;

    private bool isCatalogExpanded = false;

    private bool isFilterByAssetPacks = true;

    private const string FAVORITE_NAME = "Favorites";
    private QuickBarController quickBarController;

    private void Start()
    {
        quickBarController = new QuickBarController(quickBarView);
        favoritesController = new FavoritesController(catalogGroupListView);

        quickBarView.OnQuickBarShortcutSelected += QuickBarInput;
        catalogAssetPackListView.OnCatalogPackClick += OnCatalogItemPackSelected;
        catalogGroupListView.OnCatalogItemClicked += CatalogItemSelected;
        searchInputField.onValueChanged.AddListener(OnSearchInputChanged);

     
        quickBarController.OnCatalogItemSelected += CatalogItemSelected;

        categoryToggle.onValueChanged.AddListener(CategoryFilter);
        favoritesToggle.onValueChanged.AddListener(FavoritesFilter);
        assetPackToggle.onValueChanged.AddListener(AssetsPackFilter);
    }

    private void OnDestroy()
    {
        quickBarView.OnQuickBarShortcutSelected -= QuickBarInput;
        catalogAssetPackListView.OnCatalogPackClick -= OnCatalogItemPackSelected;
        catalogGroupListView.OnCatalogItemClicked -= CatalogItemSelected;
        if(quickBarController != null)
            quickBarController.OnCatalogItemSelected -= CatalogItemSelected;
    }

    public void AssetsPackFilter(bool isOn)
    {
        if (!isOn)
            return;
        isFilterByAssetPacks = true;
        ShowAssetsPacks();
    }

    public void CategoryFilter(bool isOn)
    {
        if (!isOn)
            return;
        isFilterByAssetPacks = false;
        ShowCategories();
    }

    public void FavoritesFilter(bool isOn)
    {
        if (!isOn)
            return;

        ShowFavorites();
    }

    public void ToggleCatalogExpanse()
    {
        if(isCatalogExpanded)
        {
            BuilderInWorldUtils.CopyRectTransform(panelRT, panelMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(headerRT, headerMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(searchBarRT, searchBarMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(assetPackRT, assetPackMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(categoryRT, assetPackMinSizeRT);
        }
        else
        {
            BuilderInWorldUtils.CopyRectTransform(panelRT, panelMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(headerRT, headerMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(searchBarRT, searchBarMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(assetPackRT, assetPackMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(categoryRT, assetPackMaxSizeRT);
        }

        isCatalogExpanded = !isCatalogExpanded;
    }

    #region Filter
    void OnSearchInputChanged(string currentSearchInput)
    {
        if (string.IsNullOrEmpty(currentSearchInput))
        {
            ShowAssetsPacks();
        }
        else
        {
            ShowCatalogContent();
            FilterAssets(currentSearchInput);
            catalogGroupListView.SetContent(filterObjects);
        }
        lastFilterName = currentSearchInput;
    }

    void FilterAssets(string nameToFilter)
    {
        filterObjects.Clear();
        foreach (CatalogItemPack assetPack in BIWCatalogManager.GetCatalogItemPackList())
        {
            foreach (CatalogItem catalogItem in assetPack.assets)
            {
                if (catalogItem.category.Contains(nameToFilter) || catalogItem.tags.Contains(nameToFilter) || catalogItem.name.Contains(nameToFilter))
                {
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
            }
        }
    }

    void AddNewSceneObjectCategoryToFilter(CatalogItem catalogItem)
    {
        Dictionary<string, List<CatalogItem>> groupedCatalogItems = new Dictionary<string, List<CatalogItem>>();
        groupedCatalogItems.Add(catalogItem.category, new List<CatalogItem>() { catalogItem });
        filterObjects.Add(groupedCatalogItems);
    }

    #endregion

    void QuickBarInput(int quickBarSlot)
    {
        quickBarController.QuickBarObjectSelected(quickBarSlot);
    }

    public void ToggleFavorites()
    {
        isFavoriteFilterActive = !isFavoriteFilterActive;

        if (!isFavoriteFilterActive)
        {
            ShowAssetsPacks();
            return;
        }
        ShowFavorites();
    }

    void ShowFavorites()
    {
        catalogTitleTxt.text = FAVORITE_NAME;
        ShowCatalogContent();

        List<Dictionary<string, List<CatalogItem>>> favorites = new List<Dictionary<string, List<CatalogItem>>>();
        Dictionary<string, List<CatalogItem>> groupedCategoryItems = new Dictionary<string, List<CatalogItem>>();
        groupedCategoryItems.Add(FAVORITE_NAME, favoritesController.GetFavorites());
        favorites.Add(groupedCategoryItems);

        catalogGroupListView.SetContent(favorites);
    }
    
    void CatalogItemSelected(CatalogItem catalogItem)
    {
        OnCatalogItemSelected?.Invoke(catalogItem);
    }

    void OnCatalogItemPackSelected(CatalogItemPack catalogItemPack)
    {
        ShowCatalogContent();

        SetCatalogAssetPackInListView(catalogItemPack);
    }

    void SetCatalogAssetPackInListView(CatalogItemPack catalogItemPack)
    {
        catalogTitleTxt.text = catalogItemPack.title;
        Dictionary<string, List<CatalogItem>> groupedCatalogItem = new Dictionary<string, List<CatalogItem>>();

        foreach (CatalogItem sceneObject in catalogItemPack.assets)
        {
            string titleToUse = sceneObject.categoryName;

            if (!groupedCatalogItem.ContainsKey(titleToUse))
            {          
                groupedCatalogItem.Add(titleToUse, GetAssetsListByCategory(titleToUse, catalogItemPack));
            }
        }

        List<Dictionary<string, List<CatalogItem>>> contentList = new List<Dictionary<string, List<CatalogItem>>>
        {
            groupedCatalogItem
        };

        catalogGroupListView.SetContent(contentList);
    }

    public void Back()
    {
        if (isShowingAssetPacks)
        {
            CloseCatalog();
        }
        else
        {
            if (isFilterByAssetPacks)
                ShowAssetsPacks();
            else
                ShowCategories();
        }

        isFavoriteFilterActive = false;
    }

    public bool IsCatalogOpen()
    {
        return gameObject.activeSelf;
    }

    public void ShowCategories()
    {
        catalogAssetPackListView.SetCategoryStyle();
        catalogAssetPackListView.SetContent(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories());
        isShowingAssetPacks = true;
        catalogTitleTxt.text = BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE;
        catalogAssetPackListView.gameObject.SetActive(true);
        catalogGroupListView.gameObject.SetActive(false);
    }

    public void ShowAssetsPacks()
    {
        catalogAssetPackListView.SetAssetPackStyle();
        catalogAssetPackListView.SetContent(BIWCatalogManager.GetCatalogItemPackList());
        isShowingAssetPacks = true;
        catalogTitleTxt.text = BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE;
        RefreshCatalog();
        catalogAssetPackListView.gameObject.SetActive(true);
        catalogGroupListView.gameObject.SetActive(false);
    }

    public void ShowCatalogContent()
    {
        isShowingAssetPacks = false;
        catalogAssetPackListView.gameObject.SetActive(false);
        catalogGroupListView.gameObject.SetActive(true);
    }

    public void OpenCatalog()
    {
        RefreshCatalog();
        catalogTitleTxt.text = BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE;
        Utils.UnlockCursor();
        gameObject.SetActive(true);   
    }

    public void CloseCatalog()
    {
        if(gameObject.activeSelf)
            StartCoroutine(CloseCatalogAfterOneFrame());
    }

    public void RefreshAssetPack()
    {
        catalogGroupListView.RefreshDisplay();
    }

    public void RefreshCatalog()
    {
        catalogAssetPackListView.SetContent(BIWCatalogManager.GetCatalogItemPackList());
    }

    List<CatalogItem> GetAssetsListByCategory(string category, CatalogItemPack sceneAssetPack)
    {
        List<CatalogItem> catalogItemList = new List<CatalogItem>();

        foreach (CatalogItem catalogItem in sceneAssetPack.assets)
        {
            if (category == catalogItem.categoryName)
                catalogItemList.Add(catalogItem);
        }

        return catalogItemList;
    }

    IEnumerator CloseCatalogAfterOneFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }
}
