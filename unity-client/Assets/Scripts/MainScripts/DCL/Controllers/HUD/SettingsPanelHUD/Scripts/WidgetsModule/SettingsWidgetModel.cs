using DCL.SettingsPanelHUD.Controls;
using ReorderableList;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    /// <summary>
    /// Model that represents a WIDGET. It contains a list of CONTROLS.
    /// </summary>
    [CreateAssetMenu(menuName = "Settings/Configuration/Widget", fileName = "WidgetConfiguration")]
    public class SettingsWidgetModel : ScriptableObject
    {
        [Tooltip("Title that will appear on the top of the widget.")]
        public string title;

        [Tooltip("Template prefab that will represent the widget.")]
        public SettingsWidgetView widgetPrefab;

        [Tooltip("Widget controller that will be associated to the widgetPrefab.")]
        public SettingsWidgetController widgetController;

        [Reorderable]
        public SettingsControlGroupList controlColumns;
    }
}