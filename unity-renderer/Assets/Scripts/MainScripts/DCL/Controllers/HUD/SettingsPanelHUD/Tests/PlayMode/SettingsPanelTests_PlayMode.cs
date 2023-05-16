using DCL.SettingsPanelHUD;
using DCL.SettingsPanelHUD.Sections;
using NSubstitute;
using SettingsSectionTests;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace SettingsPanelTests
{
    public class SettingsPanelShould_PlayMode
    {
        private const string SECTION_VIEW_PREFAB_PATH = SettingsSectionShould_PlayMode.SECTION_VIEW_PREFAB_PATH;
        private const string MENU_BUTTON_PREFAB_PATH = "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SettingsPanelHUD/Sections/DefaultSettingsMenuButtonTemplate.prefab";

        private SettingsPanelHUDView panelView;
        private IHUD hudController;
        private ISettingsPanelHUDController panelController;
        private SettingsSectionList sectionsToCreate = new SettingsSectionList();

        [UnitySetUp]
        private IEnumerator SetUp()
        {
            panelView = SettingsPanelHUDView.Create();
            hudController = Substitute.For<IHUD>();
            panelController = Substitute.For<ISettingsPanelHUDController>();

            yield return null;
        }

        [UnityTearDown]
        private IEnumerator TearDown()
        {
            Object.Destroy(panelView.gameObject);

            foreach (var section in sectionsToCreate)
            {
                Object.Destroy(section.icon);
            }
            sectionsToCreate.Clear();

            yield return null;
        }

        [UnityTest]
        public IEnumerator GenerateSectionsIntoThePanelViewCorrectly()
        {
            // Arrange
            SettingsSectionView sectionViewPrefab = AssetDatabase.LoadAssetAtPath<SettingsSectionView>(SECTION_VIEW_PREFAB_PATH);
            SettingsButtonEntry menuButtonPrefab = AssetDatabase.LoadAssetAtPath<SettingsButtonEntry>(MENU_BUTTON_PREFAB_PATH);

            SettingsSectionModel newSectionConfig = ScriptableObject.CreateInstance<SettingsSectionModel>();
            newSectionConfig.icon = Sprite.Create(new Texture2D(10, 10), new Rect(), new Vector2());
            newSectionConfig.text = "TestSection";
            newSectionConfig.menuButtonPrefab = menuButtonPrefab;
            newSectionConfig.sectionPrefab = sectionViewPrefab;
            newSectionConfig.sectionController = ScriptableObject.CreateInstance<SettingsSectionController>();
            newSectionConfig.widgets = new SettingsWidgetList();

            sectionsToCreate.Add(newSectionConfig);

            // Act
            panelView.Initialize(hudController, panelController, sectionsToCreate);
            yield return null;

            // Assert
            panelController.Received(1)
                           .AddSection(
                               Arg.Any<SettingsButtonEntry>(),
                               Arg.Any<ISettingsSectionView>(),
                               Arg.Any<ISettingsSectionController>(),
                               Arg.Any<SettingsSectionModel>());

            panelController.Received(1).OpenSection(0);
            panelController.Received(1).MarkMenuButtonAsSelected(0);
        }
    }
}
