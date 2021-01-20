using DCL.SettingsControls;
using ReorderableList;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [System.Serializable]
    public class SettingsControlGroupList : ReorderableArray<SettingsControlGroup>
    {
    }

    [System.Serializable]
    public class SettingsControlList : ReorderableArray<SettingsControlModel>
    {
    }

    [System.Serializable]
    public class SettingsControlGroup
    {
        [Reorderable]
        public SettingsControlList controls;
    }

    /// <summary>
    /// Model that represents the base of a CONTROL.
    /// </summary>
    public class SettingsControlModel : ScriptableObject
    {
        [Header("CONTROL CONFIGURATION")]
        [Tooltip("Title that will appear on the top of the control.")]
        public string title;

        [Tooltip("Template prefab that will represent the control.")]
        public SettingsControlView controlPrefab;

        [Tooltip("Control controller that will be associated to the controlPrefab.")]
        public SettingsControlController controlController;

        [Tooltip("List of boolean flags that being true will disable the control.")]
        public List<BooleanVariable> flagsThatDisableMe;

        [Tooltip("List of boolean flags that being true will deactivate the game object of the control.")]
        public List<BooleanVariable> flagsThatDeactivateMe;
        public bool isBeta;
    }
}