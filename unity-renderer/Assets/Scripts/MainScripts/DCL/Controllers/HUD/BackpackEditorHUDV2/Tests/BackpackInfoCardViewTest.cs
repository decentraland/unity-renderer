using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackInfoCardViewTest
    {
        private InfoCardComponentView infoCard;

        [SetUp]
        public void SetUp()
        {
            infoCard = Object.Instantiate(AssetDatabase.LoadAssetAtPath<InfoCardComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Prefabs/BackpackInfoCard.prefab"));
        }

        [TearDown]
        public void TearDown()
        {
            infoCard.Dispose();
            Object.Destroy(infoCard);
        }

        [Test]
        public void SetName()
        {
            infoCard.SetName("testName");

            Assert.AreEqual("testName", infoCard.wearableName.text);
        }

        [Test]
        public void SetDescription()
        {
            infoCard.SetDescription("test description");

            Assert.AreEqual("test description", infoCard.wearableDescription.text);
        }

        [Test]
        public void SetCategory()
        {
            infoCard.SetCategory("head");

            Assert.AreEqual(infoCard.categoryImage.sprite, infoCard.typeIcons.GetTypeImage("head"));
        }
    }
}
