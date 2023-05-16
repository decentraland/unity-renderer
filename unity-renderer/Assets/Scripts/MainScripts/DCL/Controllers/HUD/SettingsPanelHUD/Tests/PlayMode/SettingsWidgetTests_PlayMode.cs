using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace SettingsWidgetTests
{
    public class SettingsWidgetShould_PlayMode
    {
        private const int NUMBER_OF_COLUMNS = 2;
        public const string WIDGET_VIEW_PREFAB_PATH = "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SettingsPanelHUD/Widgets/DefaultSettingsWidgetTemplate.prefab";
        public const string CONTROL_VIEW_PREFAB_PATH = "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SettingsPanelHUD/Prefabs/Controls/{controlType}SettingsControlTemplate.prefab";

        private SettingsWidgetView widgetView;
        private ISettingsWidgetController widgetController;
        private readonly List<SettingsControlGroup> controlColumnsToCreate = new ();

        [UnitySetUp]
        private IEnumerator SetUp()
        {
            for (var i = 0; i < NUMBER_OF_COLUMNS; i++)
            {
                controlColumnsToCreate.Add(new SettingsControlGroup()
                {
                    controls = new SettingsControlList()
                });
            }

            widgetView = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(WIDGET_VIEW_PREFAB_PATH)).GetComponent<SettingsWidgetView>();
            widgetController = Substitute.For<ISettingsWidgetController>();

            yield return null;
        }

        [UnityTest]
        [TestCase(0, "Slider", ExpectedResult = null)]
        [TestCase(1, "SpinBox", ExpectedResult = null)]
        [TestCase(0, "Toggle", ExpectedResult = null)]
        public IEnumerator GenerateControlsIntoAWidgetViewCorrectly(int columnIndex, string controlType)
        {
            // Arrange
            string prefabPath = CONTROL_VIEW_PREFAB_PATH.Replace("{controlType}", controlType);
            SettingsControlView controlViewPrefab = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)).GetComponent<SettingsControlView>();

            SettingsControlModel newControlConfig = ScriptableObject.CreateInstance<SettingsControlModel>();
            newControlConfig.title = $"TestControl_Col{columnIndex}";
            newControlConfig.controlPrefab = controlViewPrefab;
            newControlConfig.controlController = ScriptableObject.CreateInstance<SettingsControlController>();
            newControlConfig.flagsThatDeactivateMe = new List<BooleanVariable>();
            newControlConfig.flagsThatDisableMe = new List<BooleanVariable>();
            newControlConfig.isBeta = false;

            controlColumnsToCreate[columnIndex].controls.Add(newControlConfig);

            // Act
            widgetView.Initialize("TestWidget", widgetController, controlColumnsToCreate);
            yield return null;

            // Assert
            widgetController.Received(1)
                            .AddControl(
                                 Arg.Any<ISettingsControlView>(),
                                 Arg.Any<SettingsControlController>(),
                                 Arg.Any<SettingsControlModel>());

            Object.Destroy(widgetView.gameObject);
            controlColumnsToCreate.Clear();
        }
    }
}
