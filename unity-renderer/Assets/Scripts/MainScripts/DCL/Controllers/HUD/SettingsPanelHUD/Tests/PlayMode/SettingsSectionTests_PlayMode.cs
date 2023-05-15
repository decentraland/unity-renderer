using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Sections;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using SettingsWidgetTests;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace SettingsSectionTests
{

    public class SettingsSectionShould_PlayMode
    {
        public const string SECTION_VIEW_PREFAB_PATH = "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SettingsPanelHUD/Sections/DefaultSettingsSectionTemplate.prefab";
        private const string WIDGET_VIEW_PREFAB_PATH = SettingsWidgetShould_PlayMode.WIDGET_VIEW_PREFAB_PATH;

        private SettingsSectionView sectionView;
        private ISettingsSectionController sectionController;
        private List<SettingsWidgetModel> widgetsToCreate = new List<SettingsWidgetModel>();

        [UnitySetUp]
        private IEnumerator SetUp()
        {
            sectionView = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(SECTION_VIEW_PREFAB_PATH)).GetComponent<SettingsSectionView>();
            sectionController = Substitute.For<ISettingsSectionController>();

            yield return null;
        }

        [UnityTearDown]
        private IEnumerator TearDown()
        {
            Object.Destroy(sectionView.gameObject);
            widgetsToCreate.Clear();

            yield return null;
        }

        [UnityTest]
        public IEnumerator GenerateWidgetIntoASectionViewCorrectly()
        {
            // Arrange
            SettingsWidgetModel newWidgetConfig = ScriptableObject.CreateInstance<SettingsWidgetModel>();
            newWidgetConfig.title = "TestWidget";
            newWidgetConfig.widgetPrefab = AssetDatabase.LoadAssetAtPath<SettingsWidgetView>(WIDGET_VIEW_PREFAB_PATH);
            newWidgetConfig.widgetController = ScriptableObject.CreateInstance<SettingsWidgetController>();
            newWidgetConfig.controlColumns = new SettingsControlGroupList();

            widgetsToCreate.Add(newWidgetConfig);

            // Act
            sectionView.Initialize(sectionController, widgetsToCreate, "test");
            yield return null;

            // Assert
            sectionController.Received(1)
                             .AddWidget(
                                 Arg.Any<ISettingsWidgetView>(),
                                 Arg.Any<ISettingsWidgetController>(),
                                 Arg.Any<SettingsWidgetModel>());
        }
    }
}
