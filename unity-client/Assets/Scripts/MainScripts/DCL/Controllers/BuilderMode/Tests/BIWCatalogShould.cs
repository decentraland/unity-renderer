using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Tests;
using UnityEngine.TestTools;

public class BIWCatalogShould
{
    private GameObject gameObjectToUse;

    [UnitySetUp]
    protected IEnumerator SetUp()
    {
        BIWCatalogManager.Init();
        gameObjectToUse = new GameObject();
        gameObjectToUse.AddComponent<AssetCatalogBridge>();
        yield return null;
    }

    [Test]
    public void BuilderInWorldSearch()
    {

        string nameToFilter = "Sandy";
        BuilderInWorldTestHelper.CreateTestCatalogLocalMultipleFloorObjects();

        CatalogItem catalogItemToFilter = null;
        foreach (CatalogItem catalogItem in DataStore.i.builderInWorld.catalogItemDict.GetValues())
        {
            if (catalogItem.name.Contains(nameToFilter))
            {
                catalogItemToFilter = catalogItem;
                return;
            }
        }

        SceneObjectCatalogController sceneObjectCatalogController = Utils.GetOrCreateComponent<SceneObjectCatalogController>(gameObjectToUse);
        List<Dictionary<string, List<CatalogItem>>>  result = sceneObjectCatalogController.FilterAssets(nameToFilter);

        CatalogItem filteredItem =  result[0].Values.ToList()[0][0];

        Assert.AreEqual(filteredItem, catalogItemToFilter);
    }

    [Test]
    public void BuilderInWorldQuickBar()
    {
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        CatalogItemAdapter adapter = BuilderInWorldTestHelper.CreateCatalogItemAdapter(gameObjectToUse);
        adapter.SetContent(item);

        CatalogAssetGroupAdapter groupAdatper = new CatalogAssetGroupAdapter();
        groupAdatper.AddAdapter(adapter);

        CatalogGroupListView catalogGroupListView = new CatalogGroupListView();
        catalogGroupListView.AddAdapter(groupAdatper);
        catalogGroupListView.generalCanvas = Utils.GetOrCreateComponent<Canvas>(gameObjectToUse);

        QuickBarView quickBarView = new QuickBarView();

        quickBarView.catalogGroupListView = catalogGroupListView;

        QuickBarController quickBarController = new QuickBarController(quickBarView);
        int slots = quickBarController.GetSlotsCount();
        quickBarView.shortcutsImgs = new QuickBarSlot[slots];

        for (int i = 0; i < slots; i++)
        {
            quickBarView.SetIndexToDrop(i);
            adapter.AdapterStartDragging(null);
            quickBarView.SceneObjectDropped(null);
            Assert.AreEqual(item, quickBarController.QuickBarObjectSelected(i));
        }

    }

    [Test]
    public void BuilderInWorldToggleFavorite()
    {
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();

        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        FavoritesController favoritesController = new FavoritesController(new CatalogGroupListView());
        favoritesController.ToggleFavoriteState(item, null);
        Assert.IsTrue(item.IsFavorite());

        favoritesController.ToggleFavoriteState(item, null);
        Assert.IsFalse(item.IsFavorite());
    }

    [Test]
    public void CatalogItemsSceneObject()
    {
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();

        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemDict.Count(), 1);
        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemPackDict.Count(), 1);
        Assert.AreEqual(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 1);
    }

    [Test]
    public void CatalogItemsNfts()
    {
        BuilderInWorldTestHelper.CreateNFT();

        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemDict.Count(), 1);
        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemPackDict.Count(), 1);
        Assert.AreEqual(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 1);
    }

    [UnityTearDown]
    protected IEnumerator TearDown()
    {
        AssetCatalogBridge.ClearCatalog();
        BIWCatalogManager.ClearCatalog();
        BIWCatalogManager.Dispose();
        if (gameObjectToUse != null)
            GameObject.Destroy(gameObjectToUse);
        yield return null;
    }
}
