using DCL.SettingsPanelHUD;
using DCL.SettingsPanelHUD.Sections;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace SettingsPanelTests
{
    public class SettingsPanelShould_EditMode
    {
        private SettingsPanelHUDController panelController;
        private ISettingsSectionView newSectionView;
        private ISettingsSectionController newSectionController;
        private SettingsSectionModel newSectionConfig;
        private Sprite testSprite;

        [SetUp]
        public void SetUp()
        {
            panelController = new SettingsPanelHUDController();
            newSectionView = Substitute.For<ISettingsSectionView>();
            newSectionController = Substitute.For<ISettingsSectionController>();
            testSprite = Sprite.Create(new Texture2D(10, 10), new Rect(), new Vector2());
            newSectionConfig = ScriptableObject.CreateInstance<SettingsSectionModel>();
            newSectionConfig.icon = testSprite;
            newSectionConfig.text = "TestSection";
            newSectionConfig.menuButtonPrefab = new GameObject().AddComponent<SettingsButtonEntry>();
            newSectionConfig.sectionPrefab = new GameObject().AddComponent<SettingsSectionView>();
            newSectionConfig.sectionController = ScriptableObject.CreateInstance<SettingsSectionController>();
            newSectionConfig.widgets = new SettingsWidgetList();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testSprite);
            panelController.sections.Clear();
        }

        [Test]
        public void AddSectionCorrectly()
        {
            // Act
            panelController.AddSection(null, newSectionView, newSectionController, newSectionConfig);

            // Assert
            newSectionView.Received(1).Initialize(newSectionController, Arg.Any<List<SettingsWidgetModel>>());
            newSectionView.Received(1).SetActive(false);
            Assert.Contains(newSectionView, panelController.sections, "The new section should be contained in the section list.");
        }

        [Test]
        public void OpenSectionCorrectly()
        {
            //Arrange
            panelController.AddSection(null, newSectionView, newSectionController, newSectionConfig);

            // Act
            panelController.OpenSection(newSectionView);

            // Assert
            newSectionView.Received(1).SetActive(true);
        }

        [Test]
        public void OpenSectionByIndexCorrectly()
        {
            //Arrange
            panelController.AddSection(null, newSectionView, newSectionController, newSectionConfig);

            // Act
            panelController.OpenSection(panelController.sections.Count - 1);

            // Assert
            newSectionView.Received(1).SetActive(true);
        }
    }
}
