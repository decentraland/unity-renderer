using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Tests.BuildModeHUDControllers
{
    public class SceneCatalogControllerShould
    {
        private SceneCatalogController sceneCatalogController;

        [SetUp]
        public void SetUp()
        {
            sceneCatalogController = new SceneCatalogController();
            sceneCatalogController.Initialize(
                Substitute.For<ISceneCatalogView>(),
                Substitute.For<IQuickBarController>());
        }

        [TearDown]
        public void TearDown() { sceneCatalogController.Dispose(); }

        [Test]
        public void ToggleCatalogExpanseCorrectly()
        {
            // Act
            sceneCatalogController.ToggleCatalogExpanse();

            // Assert
            sceneCatalogController.sceneCatalogView.Received(1).ToggleCatalogExpanse();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void QuickBarInputCorrectly(int quickBarSlot)
        {
            // Act
            sceneCatalogController.QuickBarInput(quickBarSlot);

            // Assert
            sceneCatalogController.quickBarController.Received(1).QuickBarObjectSelected(quickBarSlot);
        }

        [Test]
        public void CatalogItemSelectedCorrectly()
        {
            // Arrange
            CatalogItem testCatalogItem = new CatalogItem { id = "test id" };
            CatalogItem returnedCatalogItem = null;
            sceneCatalogController.OnCatalogItemSelected += (catalogItem) => { returnedCatalogItem = catalogItem; };

            // Act
            sceneCatalogController.CatalogItemSelected(testCatalogItem);

            // Assert
            Assert.AreEqual(testCatalogItem, returnedCatalogItem, "The catalog item does not match!");
        }

        [Test]
        public void ResumeInputCorrectly()
        {
            // Arrange
            bool resumeInput = false;
            sceneCatalogController.OnResumeInput += () => { resumeInput = true; };

            // Act
            sceneCatalogController.ResumeInput();

            // Assert
            Assert.IsTrue(resumeInput, "The resumeInput is false!");
        }

        [Test]
        public void StopInputCorrectly()
        {
            // Arrange
            bool stopInput = false;
            sceneCatalogController.OnStopInput += () => { stopInput = true; };

            // Act
            sceneCatalogController.StopInput();

            // Assert
            Assert.IsTrue(stopInput, "The stopInput is false!");
        }

        [Test]
        public void HideCatalogClickedCorrectly()
        {
            // Arrange
            bool hideCatalogClicked = false;
            sceneCatalogController.OnHideCatalogClicked += () => { hideCatalogClicked = true; };

            // Act
            sceneCatalogController.HideCatalogClicked();

            // Assert
            Assert.IsTrue(hideCatalogClicked, "The hideCatalogClicked is false!");
        }

        [Test]
        public void GetAssetsListByCategoryCorrectly()
        {
            // Arrange
            string testCategoryA = "test category A";
            string testCategoryB = "test category B";
            CatalogItemPack testDceneAssetPack = new CatalogItemPack
            {
                assets = new List<CatalogItem>
                {
                    new CatalogItem { id = "testItemId1", categoryName = testCategoryA },
                    new CatalogItem { id = "testItemId2", categoryName = testCategoryA },
                    new CatalogItem { id = "testItemId3", categoryName = testCategoryB }
                }
            };

            // Act
            List<CatalogItem> result = sceneCatalogController.GetAssetsListByCategory(testCategoryA, testDceneAssetPack);

            // Assert
            Assert.AreEqual(2, result.Count(x => x.categoryName == testCategoryA), "The number of returned catalog items does not match!");
            Assert.AreEqual(0, result.Count(x => x.categoryName == testCategoryB), "The number of returned catalog items does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SceneCatalogBackCorrectly(bool isShowingAssetPacks)
        {
            // Arrange
            sceneCatalogController.isShowingAssetPacks = isShowingAssetPacks;

            // Act
            sceneCatalogController.SceneCatalogBack();

            // Assert
            sceneCatalogController.sceneCatalogView.Received(isShowingAssetPacks ? 1 : 0).CloseCatalog();
        }

        [Test]
        public void CheckIfCatalogIsOpenCorrectly()
        {
            // Act
            sceneCatalogController.IsCatalogOpen();

            // Assert
            sceneCatalogController.sceneCatalogView.Received(1).IsCatalogOpen();
        }

        [Test]
        public void ShowCategoriesCorrectly()
        {
            // Arrange
            sceneCatalogController.isShowingAssetPacks = false;

            // Act
            sceneCatalogController.ShowCategories();

            // Assert
            Assert.IsTrue(sceneCatalogController.isShowingAssetPacks, "The isShowingAssetPacks is false!");
            sceneCatalogController.sceneCatalogView.Received(1).SetCatalogTitle(Arg.Any<string>());
        }

        [Test]
        public void ShowAssetsPacksCorrectly()
        {
            // Arrange
            sceneCatalogController.isShowingAssetPacks = false;

            // Act
            sceneCatalogController.ShowAssetsPacks();

            // Assert
            Assert.IsTrue(sceneCatalogController.isShowingAssetPacks, "The isShowingAssetPacks is false!");
            sceneCatalogController.sceneCatalogView.Received(1).SetCatalogTitle(Arg.Any<string>());
        }

        [Test]
        public void OpenCatalogCorrectly()
        {
            // Arrange
            Utils.LockCursor();

            // Act
            sceneCatalogController.OpenCatalog();

            // Assert
            sceneCatalogController.sceneCatalogView.Received(1).SetCatalogTitle(Arg.Any<string>());
            Assert.IsFalse(Utils.isCursorLocked, "The cursor is locked!");
            sceneCatalogController.sceneCatalogView.Received(1).SetActive(true);
        }

        [Test]
        public void CloseCatalogCorrectly()
        {
            // Act
            sceneCatalogController.CloseCatalog();

            // Assert
            sceneCatalogController.sceneCatalogView.Received(1).CloseCatalog();
        }
    }
}