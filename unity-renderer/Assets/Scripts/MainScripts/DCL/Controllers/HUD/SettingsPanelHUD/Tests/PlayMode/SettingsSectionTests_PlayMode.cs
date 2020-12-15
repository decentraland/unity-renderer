using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Sections;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace SettingsSectionTests
{

    public class SettingsSectionShould_PlayMode
    {
        private const string SECTION_VIEW_PREFAB_PATH = "Sections/DefaultSettingsSectionTemplate";
        private const string WIDGET_VIEW_PREFAB_PATH = "Widgets/DefaultSettingsWidgetTemplate";

        private SettingsSectionView sectionView;
        private ISettingsSectionController sectionController;
        private List<SettingsWidgetModel> widgetsToCreate = new List<SettingsWidgetModel>();

        [UnitySetUp]
        private IEnumerator SetUp()
        {
            sectionView = Object.Instantiate((GameObject)Resources.Load(SECTION_VIEW_PREFAB_PATH)).GetComponent<SettingsSectionView>();
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
            SettingsWidgetView widgetViewPrefab = ((GameObject)Resources.Load(WIDGET_VIEW_PREFAB_PATH)).GetComponent<SettingsWidgetView>();

            SettingsWidgetModel newWidgetConfig = ScriptableObject.CreateInstance<SettingsWidgetModel>();
            newWidgetConfig.title = "TestWidget";
            newWidgetConfig.widgetPrefab = widgetViewPrefab;
            newWidgetConfig.widgetController = ScriptableObject.CreateInstance<SettingsWidgetController>();
            newWidgetConfig.controlColumns = new SettingsControlGroupList();

            widgetsToCreate.Add(newWidgetConfig);

            // Act
            sectionView.Initialize(sectionController, widgetsToCreate);
            yield return null;

            // Assert
            sectionController.Received(1).AddWidget(
                Arg.Any<ISettingsWidgetView>(),
                Arg.Any<ISettingsWidgetController>(),
                Arg.Any<SettingsWidgetModel>());
        }
    }
}
