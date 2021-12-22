using System.Collections;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace Tests.BuildModeHUDControllers
{
    public class DragAndDropSceneObjectControllerShould
    {
        private DragAndDropSceneObjectController dragAndDropSceneObjectController;
        private GameObject mockedGameObject;
        private AssetCatalogBridge assetCatalogBridge;
        private CatalogItem item;

        [SetUp]
        public void SetUp()
        {
            mockedGameObject = new GameObject("DragAndDropSceneObject");
            var canvas = mockedGameObject.AddComponent<Canvas>();
            dragAndDropSceneObjectController = new DragAndDropSceneObjectController();
            var view = Substitute.For<IDragAndDropSceneObjectView>();
            view.Configure().GetGeneralCanvas().Returns(canvas);
            dragAndDropSceneObjectController.Initialize(Substitute.For<ISceneCatalogController>(), view);
            assetCatalogBridge = mockedGameObject.AddComponent<AssetCatalogBridge>();

            item = BIWTestUtils.CreateTestCatalogLocalSingleObject(assetCatalogBridge);
            item.thumbnailURL = "";
        }

        [TearDown]
        public void TearDown()
        {
            BIWCatalogManager.ClearCatalog();
            AssetCatalogBridge.i.ClearCatalog();
            Object.Destroy(assetCatalogBridge);
            Object.Destroy(mockedGameObject);
        }

        [Test]
        public void ClickCorrectly()
        {
            // Arrange
            bool dropped = false;
            dragAndDropSceneObjectController.OnDrop += () => { dropped = true; };

            // Act
            dragAndDropSceneObjectController.Drop();

            // Assert
            Assert.IsTrue(dropped, "dropped is false!");
        }

        [Test]
        public void AdapterStartDragging()
        {
            //Arrange
            var adapter = CreateAdapter();

            //Act
            dragAndDropSceneObjectController.AdapterStartDragging(item, adapter);

            //Assert
            Assert.IsNotNull(dragAndDropSceneObjectController.catalogItemCopy);
        }

        [Test]
        public void MoveCopyAdapterToPosition()
        {
            //Arrange
            var adapter = CreateAdapter();
            dragAndDropSceneObjectController.AdapterStartDragging(item, adapter);
            var newPosition = Vector3.one * 5f;

            //Act
            dragAndDropSceneObjectController.MoveCopyAdapterToPosition(newPosition);

            //Assert
            Assert.IsTrue(Vector3.Distance(dragAndDropSceneObjectController.catalogItemCopy.transform.position, newPosition) <= 0.1f);
        }

        [UnityTest]
        public IEnumerator AdapterEndDrag()
        {
            //Arrange
            var adapter = CreateAdapter();
            dragAndDropSceneObjectController.AdapterStartDragging(item, adapter);

            //Act
            dragAndDropSceneObjectController.OnEndDrag(null);

            //Assert
            //We wait 1 frame for the GameObject to be destroyed
            yield return null;
            Assert.IsTrue(dragAndDropSceneObjectController.catalogItemCopy == null);
        }

        [Test]
        public void CatalogItemAdapterDropped()
        {
            //Arrange
            var adapter = CreateAdapter();
            dragAndDropSceneObjectController.AdapterStartDragging(item, adapter);
            var newPosition = Vector3.one * 5f;

            //Act
            dragAndDropSceneObjectController.MoveCopyAdapterToPosition(newPosition);

            //Act
            dragAndDropSceneObjectController.CatalogItemDropped();

            //Assert
            Assert.IsNotNull(dragAndDropSceneObjectController.itemDroped);
        }

        private CatalogItemAdapter CreateAdapter()
        {
            var adapter = BIWTestUtils.CreateCatalogItemAdapter(mockedGameObject);
            adapter.SetContent(item);
            return adapter;
        }
    }
}