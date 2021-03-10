using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Sections;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace SettingsSectionTests
{
    public class SettingsSectionShould_EditMode
    {
		[Test]
        public void AddWidgetCorrectly()
        {
            // Arrange
            var newWidgetView = Substitute.For<ISettingsWidgetView>();
            var newWidgetController = Substitute.For<ISettingsWidgetController>();
            var newWidgetConfig = ScriptableObject.CreateInstance<SettingsWidgetModel>();
            newWidgetConfig.title = "TestWidget";
            newWidgetConfig.widgetPrefab = new GameObject().AddComponent<SettingsWidgetView>();
            newWidgetConfig.widgetController = ScriptableObject.CreateInstance<SettingsWidgetController>();
            newWidgetConfig.controlColumns = new SettingsControlGroupList();

            SettingsSectionController sectionController = ScriptableObject.CreateInstance<SettingsSectionController>();

            // Act
            sectionController.AddWidget(newWidgetView, newWidgetController, newWidgetConfig);

            // Assert
            newWidgetView.Received(1).Initialize(
                newWidgetConfig.title,
                newWidgetController,
                Arg.Any<List<SettingsControlGroup>>());

            Assert.Contains(newWidgetView, sectionController.widgets, "The new widget should be contained in the widget list.");
        }
    }
}
