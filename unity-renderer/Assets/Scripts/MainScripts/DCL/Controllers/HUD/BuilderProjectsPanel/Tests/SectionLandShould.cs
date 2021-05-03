using System.Collections.Generic;
using System.Linq;
using DCL;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SectionLandShould
    {
        private SectionLandView view;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SectionLandView>(SectionLandController.VIEW_PREFAB_PATH);
            view = Object.Instantiate(prefab);
            WebRequestController.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(view.gameObject);
            WebRequestController.i.Dispose();
        }

        [Test]
        public void PrefabSetupCorrectly()
        {
            Assert.AreEqual(1, view.GetLandElementsContainer().childCount);
            Assert.NotNull(view.landElementView);
        }
        
        [Test]
        public void SetEmptyCorrectly()
        {
            view.SetEmpty();
            Assert.IsTrue(view.emptyContainer.activeSelf);
            Assert.IsFalse(view.contentContainer.activeSelf);
            Assert.IsFalse(view.loadingAnimationContainer.activeSelf);
            Assert.IsFalse(view.noSearchResultContainer.activeSelf);
        }
        
        [Test]
        public void SetNotEmptyCorrectly()
        {
            view.SetFilled();
            Assert.IsTrue(view.contentContainer.activeSelf);
            Assert.IsFalse(view.emptyContainer.activeSelf);
            Assert.IsFalse(view.loadingAnimationContainer.activeSelf);
            Assert.IsFalse(view.noSearchResultContainer.activeSelf);
        }
        
        [Test]
        public void SetNoSearchResultCorrectly()
        {
            view.SetNoSearchResult();
            Assert.IsFalse(view.contentContainer.activeSelf);
            Assert.IsFalse(view.emptyContainer.activeSelf);
            Assert.IsFalse(view.loadingAnimationContainer.activeSelf);
            Assert.IsTrue(view.noSearchResultContainer.activeSelf);
        }
        
        [Test]
        public void SetLoadingCorrectly()
        {
            view.SetLoading();
            Assert.IsFalse(view.contentContainer.activeSelf);
            Assert.IsFalse(view.emptyContainer.activeSelf);
            Assert.IsTrue(view.loadingAnimationContainer.activeSelf);
            Assert.IsFalse(view.noSearchResultContainer.activeSelf);
        }
        
        [Test]
        public void ShowAndHideCorrectly()
        {
            SectionLandController controller = new SectionLandController(view);
            controller.SetVisible(true);
            
            controller.SetVisible(false);
            Assert.IsFalse(view.gameObject.activeSelf);
            
            controller.SetVisible(true);
            Assert.IsTrue(view.gameObject.activeSelf);
            controller.Dispose();
        }

        [Test]
        public void SetLandsCorrectly()
        {
            SectionLandController controller = new SectionLandController(view);
            ILandsListener landsListener = controller;

            landsListener.OnSetLands(new [] { CreateLandData("1"), CreateLandData("2") });
            Assert.AreEqual(2, GetVisibleChildrenAmount(view.GetLandElementsContainer()));
            Assert.IsTrue(view.contentContainer.activeSelf);

            landsListener.OnSetLands(new [] { CreateLandData("1") });
            Assert.AreEqual(1, GetVisibleChildrenAmount(view.GetLandElementsContainer()));
            Assert.IsTrue(view.contentContainer.activeSelf);

            landsListener.OnSetLands(new LandWithAccess[] { });
            Assert.AreEqual(0, GetVisibleChildrenAmount(view.GetLandElementsContainer()));
            Assert.IsFalse(view.contentContainer.activeSelf);
            Assert.IsTrue(view.emptyContainer.activeSelf);

            controller.Dispose();
        }

        [Test]
        public void UpdateLandCorrectly()
        {
            const string startingName = "Temptation";
            const string updatedName = "New Temptation";

            SectionLandController controller = new SectionLandController(view);
            ILandsListener landsListener = controller;
            LandElementView landElementView = view.GetLandElementeBaseView();

            landsListener.OnSetLands(new [] { CreateLandData("1", startingName) });
            Assert.AreEqual(1, GetVisibleChildrenAmount(view.GetLandElementsContainer()));
            Assert.AreEqual(startingName, landElementView.landName.text);

            landsListener.OnSetLands(new [] { CreateLandData("1", updatedName) });
            Assert.AreEqual(1, GetVisibleChildrenAmount(view.GetLandElementsContainer()));
            Assert.AreEqual(updatedName, landElementView.landName.text);


            controller.Dispose();
        }

        private int GetVisibleChildrenAmount(Transform parent)
        {
            return parent.Cast<Transform>().Count(child => child.gameObject.activeSelf);
        }

        private LandWithAccess CreateLandData(string id)
        {
            return new LandWithAccess(
                new Land()
                {
                    id = id,
                    parcels = new List<Parcel>()
                }
            );
        }
        
        private LandWithAccess CreateLandData(string id, string name)
        {
            return new LandWithAccess(
                new Land()
                {
                    id = id,
                    name = name,
                    parcels = new List<Parcel>()
                }
            );
        }        
    }
}