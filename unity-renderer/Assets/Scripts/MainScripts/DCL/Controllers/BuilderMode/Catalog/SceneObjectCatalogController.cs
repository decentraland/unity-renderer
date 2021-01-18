
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
    public event System.Action<SceneObject> OnSceneObjectSelected;
    public event System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;


    public TextMeshProUGUI catalogTitleTxt;
    public CatalogAssetPackListView catalogAssetPackListView;
    public CatalogGroupListView catalogGroupListView;
    public TMP_InputField searchInputField;
    public FavoritesController favoritesController;
    public QuickBarView quickBarView;


    List<Dictionary<string, List<SceneObject>>> filterObjects = new List<Dictionary<string, List<SceneObject>>>();
    string lastFilterName = "";
    bool catalogInitializaed = false, isShowingAssetPacks = false, isFavoriteFilterActive = false;


    const string favoriteName = "Favorites";
    QuickBarController quickBarController;

    private void Start()
    {
        quickBarController = new QuickBarController(quickBarView);
        favoritesController = new FavoritesController(catalogGroupListView);

        quickBarView.OnQuickBarShortcutSelected += QuickBarInput;
        catalogAssetPackListView.OnSceneAssetPackClick += OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked += SceneObjectSelected;
        searchInputField.onValueChanged.AddListener(OnSearchInputChanged);

           
        quickBarController.OnSceneObjectSelected += SceneObjectSelected;
    }

    private void OnDestroy()
    {
        quickBarView.OnQuickBarShortcutSelected -= QuickBarInput;
        catalogAssetPackListView.OnSceneAssetPackClick -= OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked -= SceneObjectSelected;
        quickBarController.OnSceneObjectSelected -= SceneObjectSelected;
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
        foreach (SceneAssetPack assetPack in AssetCatalogBridge.sceneAssetPackCatalog.GetValues().ToList())
        {
            foreach (SceneObject sceneObject in assetPack.assets)
            {
                if (sceneObject.category.Contains(nameToFilter) || sceneObject.tags.Contains(nameToFilter) || sceneObject.name.Contains(nameToFilter))
                {
                    bool foundCategory = false;
                    foreach (Dictionary<string, List<SceneObject>> groupedSceneObjects in filterObjects)
                    {
                        if (groupedSceneObjects.ContainsKey(sceneObject.category))
                        {
                            foundCategory = true;
                            if (!groupedSceneObjects[sceneObject.category].Contains(sceneObject))
                                groupedSceneObjects[sceneObject.category].Add(sceneObject);
                        }
                    }
                    if (!foundCategory)
                    {
                        AddNewSceneObjectCategoryToFilter(sceneObject);
                    }
                }
            }
        }
    }

    void AddNewSceneObjectCategoryToFilter(SceneObject sceneObject)
    {
        Dictionary<string, List<SceneObject>> groupedSceneObjects = new Dictionary<string, List<SceneObject>>();
        groupedSceneObjects.Add(sceneObject.category, new List<SceneObject>() { sceneObject });
        filterObjects.Add(groupedSceneObjects);
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
        catalogTitleTxt.text = favoriteName;
        ShowCatalogContent();

        List<Dictionary<string, List<SceneObject>>> favorites = new List<Dictionary<string, List<SceneObject>>>();
        Dictionary<string, List<SceneObject>> groupedSceneObjects = new Dictionary<string, List<SceneObject>>();
        groupedSceneObjects.Add(favoriteName, favoritesController.GetFavorites());
        favorites.Add(groupedSceneObjects);

        catalogGroupListView.SetContent(favorites);
    }

    void SceneObjectSelected(SceneObject sceneObject)
    {
        OnSceneObjectSelected?.Invoke(sceneObject);
    }

    void OnScenePackSelected(SceneAssetPack sceneAssetPack)
    {
        ShowCatalogContent();

        SetAssetPackInListView(sceneAssetPack);
    }

    void SetAssetPackInListView(SceneAssetPack sceneAssetPack)
    {
        catalogTitleTxt.text = sceneAssetPack.title;
        Dictionary<string, List<SceneObject>> groupedSceneObjects = new Dictionary<string, List<SceneObject>>();

        foreach (SceneObject sceneObject in sceneAssetPack.assets)
        {
            if (!groupedSceneObjects.ContainsKey(sceneObject.category))
            {
                groupedSceneObjects.Add(sceneObject.category, GetAssetsListByCategory(sceneObject.category, sceneAssetPack));
            }
        }

        List<Dictionary<string, List<SceneObject>>> contentList = new List<Dictionary<string, List<SceneObject>>>
        {
            groupedSceneObjects
        };

        catalogGroupListView.SetContent(contentList);
    }

    public void Back()
    {
        if (isShowingAssetPacks)
            CloseCatalog();
        else
            ShowAssetsPacks();

        isFavoriteFilterActive = false;
    }

    public bool IsCatalogOpen()
    {
        return gameObject.activeSelf;
    }

    public void ShowAssetsPacks()
    {
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
        catalogAssetPackListView.SetContent(GetContentForCatalog());
    }

    List<SceneAssetPack> GetContentForCatalog()
    {
        List<SceneAssetPack> catalogList =  AssetCatalogBridge.sceneAssetPackCatalog.GetValues().ToList();
        catalogList.Add(GetCollectiblesAssetPack());
        return catalogList;
    }

    SceneAssetPack GetCollectiblesAssetPack()
    {
        SceneAssetPack sceneAssetPack = new SceneAssetPack();
        sceneAssetPack.id = BuilderInWorldSettings.ASSETS_COLLECTIBLES;
        sceneAssetPack.title = BuilderInWorldSettings.ASSETS_COLLECTIBLES;

        sceneAssetPack.assets = BuilderInWorldNFTController.i.GetNFTsAsSceneObjects();      
        return sceneAssetPack;
    }

    List<SceneObject> GetAssetsListByCategory(string category, SceneAssetPack sceneAssetPack)
    {
        List<SceneObject> sceneObjectsList = new List<SceneObject>();

        foreach (SceneObject sceneObject in sceneAssetPack.assets)
        {
            if (category == sceneObject.category) sceneObjectsList.Add(sceneObject);
        }

        return sceneObjectsList;
    }

    IEnumerator CloseCatalogAfterOneFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }
 
}
