using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWCatalogShould : IntegrationTestSuite_Legacy
{
    private AssetCatalogBridge assetCatalogBridge;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        BIWCatalogManager.Init();
        assetCatalogBridge = TestUtils.CreateComponentWithGameObject<AssetCatalogBridge>("AssetCatalogBridge");
        yield return null;
    }

    protected override ServiceLocator InitializeServiceLocator()
    {
        return DCL.ServiceLocatorTestFactory.CreateMocked();
    }


    [Test]
    public void BuilderInWorldSearch()
    {
        string nameToFilter = "Sandy";
        BIWTestUtils.CreateTestCatalogLocalMultipleFloorObjects(assetCatalogBridge);

        CatalogItem catalogItemToFilter = null;
        foreach (CatalogItem catalogItem in DataStore.i.builderInWorld.catalogItemDict.GetValues())
        {
            if (catalogItem.name.Contains(nameToFilter))
            {
                catalogItemToFilter = catalogItem;
                return;
            }
        }

        SceneCatalogController sceneCatalogController = new SceneCatalogController();
        List<Dictionary<string, List<CatalogItem>>>  result = sceneCatalogController.biwSearchBarController.FilterAssets(nameToFilter);

        CatalogItem filteredItem =  result[0].Values.ToList()[0][0];

        Assert.AreEqual(filteredItem, catalogItemToFilter);
    }

    [Test]
    public void BuilderInWorldQuickBar()
    {
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        CatalogItemAdapter adapter = BIWTestUtils.CreateCatalogItemAdapter(assetCatalogBridge.gameObject);
        adapter.SetContent(item);

        CatalogAssetGroupAdapter groupAdapter = new GameObject("_CatalogAssetGroupAdapter").AddComponent<CatalogAssetGroupAdapter>();
        groupAdapter.SubscribeToEvents(adapter);

        CatalogGroupListView catalogGroupListView = new GameObject("_CatalogGroupListView").AddComponent<CatalogGroupListView>();
        catalogGroupListView.SubscribeToEvents(groupAdapter);
        catalogGroupListView.generalCanvas = assetCatalogBridge.gameObject.GetOrCreateComponent<Canvas>();
        SceneCatalogView sceneCatalogView = SceneCatalogView.Create();
        sceneCatalogView.catalogGroupListView = catalogGroupListView;
        SceneCatalogController sceneCatalogController = new SceneCatalogController();

        QuickBarView quickBarView = QuickBarView.Create();

        QuickBarController quickBarController = new QuickBarController();
        sceneCatalogController.Initialize(sceneCatalogView, quickBarController);
        quickBarController.Initialize(quickBarView, Substitute.For<IDragAndDropSceneObjectController>());
        int slots = quickBarController.GetSlotsCount();
        quickBarView.shortcutsImgs = new QuickBarSlot[slots];

        for (int i = 0; i < slots; i++)
        {
            quickBarController.SetIndexToDrop(i);
            adapter.AdapterStartDragging(null);
            quickBarController.SetQuickBarShortcut(item, i, new Texture2D(10, 10));
            Assert.AreEqual(item, quickBarController.QuickBarObjectSelected(i));
        }
    }

    [Test]
    public void BuilderInWorldToggleFavorite()
    {
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);

        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        FavoritesController favoritesController = new FavoritesController(new GameObject("_FavoritesController").AddComponent<CatalogGroupListView>());
        favoritesController.ToggleFavoriteState(item, null);
        Assert.IsTrue(item.IsFavorite());

        favoritesController.ToggleFavoriteState(item, null);
        Assert.IsFalse(item.IsFavorite());
    }

    [Test]
    public void CatalogItemsSceneObject()
    {
        BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);

        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemDict.Count(), 1);
        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemPackDict.Count(), 1);
        Assert.AreEqual(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 1);
    }

    [Test]
    public void CatalogItemsNfts()
    {
        BIWTestUtils.CreateNFT();

        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemDict.Count(), 1);
        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemPackDict.Count(), 1);
        Assert.AreEqual(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 1);
    }

    [UnityTearDown]
    protected IEnumerator TearDown()
    {
        AssetCatalogBridge.i.ClearCatalog();
        BIWCatalogManager.ClearCatalog();
        BIWCatalogManager.Dispose();
        UnityEngine.Object.Destroy(assetCatalogBridge.gameObject);
        yield return null;
    }
}