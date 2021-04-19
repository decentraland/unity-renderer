using DCL.SettingsControls;
using DCL.SettingsPanelHUD.Controls;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    /// <summary>
    /// Interface to implement a controller for a WIDGET.
    /// </summary>
    public interface ISettingsWidgetController
    {
        /// <summary>
        /// List of all CONTROLS added to the WIDGET.
        /// </summary>
        List<ISettingsControlView> controls { get; }

        /// <summary>
        /// Adds a CONTROL into the WIDGET.
        /// </summary>
        /// <param name="newControl">New CONTROL that will be added.</param>
        /// <param name="newControlController">Controller belonging to the new CONTROL.</param>
        /// <param name="controlConfig">Model that will contain the configuration of the new CONTROL.</param>
        void AddControl(ISettingsControlView newControl, SettingsControlController newControlController, SettingsControlModel controlConfig);
    }

    /// <summary>
    /// This controller is in charge of manage all the logic related to a WIDGET.
    /// </summary>
    [CreateAssetMenu(menuName = "Settings/Controllers/Widget", fileName = "SettingsWidgetController")]
    public class SettingsWidgetController : ScriptableObject, ISettingsWidgetController
    {
        public List<ISettingsControlView> controls { get; } = new List<ISettingsControlView>();

        public void AddControl(
            ISettingsControlView newControl,
            SettingsControlController newControlController,
            SettingsControlModel controlConfig)
        {
            newControl.Initialize(controlConfig, newControlController);
            controls.Add(newControl);
        }
    }
}