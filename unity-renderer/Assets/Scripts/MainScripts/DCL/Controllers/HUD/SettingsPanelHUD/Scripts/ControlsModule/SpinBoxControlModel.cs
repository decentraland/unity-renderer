using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// Model that represents a SPIN-BOX type CONTROL.
    /// </summary>
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/SpinBox Control", fileName = "SpinBoxControlConfiguration")]
    public class SpinBoxControlModel : SettingsControlModel
    {
        [Header("SPIN-BOX CONFIGURATION")]
        [Tooltip("List of available options for the spin-box.")]
        public string[] spinBoxLabels;
    }
}