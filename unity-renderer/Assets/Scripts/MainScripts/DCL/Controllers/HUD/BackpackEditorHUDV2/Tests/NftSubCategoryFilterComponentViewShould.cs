using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Backpack
{
    public class NftSubCategoryFilterComponentViewShould
    {
        private NftSubCategoryFilterComponentView view;
        private Sprite sprite;
        private Texture2D texture;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(AssetDatabase.LoadAssetAtPath<NftSubCategoryFilterComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Prefabs/NftBreadcrumberSubcategoryFilter.prefab"));

            texture = new Texture2D(1, 1);
            sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
            Object.Destroy(sprite);
            Object.Destroy(texture);
        }

        [Test]
        public void ShowCategoryNameWithResultCount()
        {
            view.SetModel(new NftSubCategoryFilterModel
            {
                Filter = "all",
                Icon = sprite,
                IsSelected = false,
                Name = "All",
                ResultCount = 15,
                ShowResultCount = true,
            });

            Assert.AreEqual("All (15)", view.categoryName.text);
        }

        [Test]
        public void ShowCategoryName()
        {
            view.SetModel(new NftSubCategoryFilterModel
            {
                Filter = "name=something",
                Icon = sprite,
                IsSelected = false,
                Name = "something",
                ResultCount = 4,
                ShowResultCount = false,
            });

            Assert.AreEqual("something", view.categoryName.text);
        }

        [Test]
        public void SetIcon()
        {
            view.SetModel(new NftSubCategoryFilterModel
            {
                Filter = "all",
                Icon = sprite,
                IsSelected = false,
                Name = "All",
                ResultCount = 15,
                ShowResultCount = true,
            });

            Assert.AreEqual(view.icon.sprite, sprite);
        }
    }
}
