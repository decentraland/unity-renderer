using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Backpack
{
    public class NftBreadcrumbComponentViewShould
    {
        private NftBreadcrumbComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(AssetDatabase.LoadAssetAtPath<NftBreadcrumbComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Prefabs/WearablesBreadcrumb.prefab"));
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        public void AddSubCategories()
        {
            view.SetModel(new NftBreadcrumbModel
            {
                Current = 0,
                ResultCount = 42,
                Path = new (string Filter, string Name, string Type, bool Removable)[]
                {
                    ("all", "All", "all", false),
                    ("name=something", "something", "name", true),
                }
            });

            NftSubCategoryFilterComponentView[] subCategories = view.container.GetComponentsInChildren<NftSubCategoryFilterComponentView>();

            Assert.AreEqual(2, subCategories.Length);

            NftSubCategoryFilterModel modelAll = subCategories[0].Model;
            Assert.AreEqual("All", modelAll.Name);
            Assert.AreEqual("all", modelAll.Filter);
            Assert.AreEqual(42, modelAll.ResultCount);
            Assert.IsTrue(modelAll.IsSelected);
            Assert.IsFalse(modelAll.ShowResultCount);

            NftSubCategoryFilterModel modelByName = subCategories[1].Model;
            Assert.AreEqual("something", modelByName.Name);
            Assert.AreEqual("name=something", modelByName.Filter);
            Assert.AreEqual(42, modelByName.ResultCount);
            Assert.IsFalse(modelByName.IsSelected);
            // the result count is not shown anymore
            // Assert.IsTrue(modelByName.ShowResultCount);
            Assert.IsFalse(modelByName.ShowResultCount);
        }

        [Test]
        public void ReplaceSubCategories()
        {
            view.SetModel(new NftBreadcrumbModel
            {
                Current = 0,
                ResultCount = 42,
                Path = new (string Filter, string Name, string Type, bool Removable)[]
                {
                    ("all", "All", "all", false),
                    ("name=something", "something", "name", true),
                }
            });

            view.SetModel(new NftBreadcrumbModel
            {
                Current = 0,
                ResultCount = 3,
                Path = new (string Filter, string Name, string Type, bool Removable)[]
                {
                    ("category=shoes", "Shoes", "shoes", true),
                }
            });

            NftSubCategoryFilterComponentView[] subCategories = view.container.GetComponentsInChildren<NftSubCategoryFilterComponentView>();

            Assert.AreEqual(1, subCategories.Length);

            NftSubCategoryFilterModel model = subCategories[0].Model;
            Assert.AreEqual("Shoes", model.Name);
            Assert.AreEqual("category=shoes", model.Filter);
            Assert.AreEqual(3, model.ResultCount);
            Assert.IsTrue(model.IsSelected);
            // Assert.IsTrue(model.ShowResultCount);
            // the result count is not shown anymore
            Assert.IsFalse(model.ShowResultCount);
        }
    }
}
