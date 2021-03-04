using DCL.SettingsPanelHUD.Widgets;
using ReorderableList;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    [System.Serializable]
    public class SettingsWidgetList : ReorderableArray<SettingsWidgetModel>
    {
    }

    /// <summary>
    /// Model that represents a SECTION. It contains a list of WIDGETS.
    /// </summary>
    [CreateAssetMenu(menuName = "Settings/Configuration/Section", fileName = "SectionConfiguration")]
    public class SettingsSectionModel : ScriptableObject
    {
        [Tooltip("Icon that will appear before the button text.")]
        public Sprite icon;

        [Tooltip("Title that will appear on the top of the section and in the associated menu button.")]
        public string text;

        [Tooltip("Template prefab that will represent the menu button associated to this section.")]
        public SettingsButtonEntry menuButtonPrefab;

        [Tooltip("Template prefab that will represent the section.")]
        public SettingsSectionView sectionPrefab;

        [Tooltip("Section controller that will be associated to the sectionPrefab.")]
        public SettingsSectionController sectionController;

        [Reorderable]
        public SettingsWidgetList widgets;
    }
}