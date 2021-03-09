using DCL.SettingsPanelHUD.Widgets;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DCL.SettingsPanelHUD.Sections
{
    /// <summary>
    /// Interface to implement a controller for a SECTION.
    /// </summary>
    public interface ISettingsSectionController
    {
        /// <summary>
        /// List of all WIDGETS added to the SECTION.
        /// </summary>
        List<ISettingsWidgetView> widgets { get; }

        /// <summary>
        /// Adds a WIDGET into the SECTION.
        /// </summary>
        /// <param name="newWidget">New WIDGET that will be added.</param>
        /// <param name="newWidgetController">Controller belonging to the new WIDGET.</param>
        /// <param name="widgetConfig">Model that will contain the configuration of the new WIDGET.</param>
        void AddWidget(ISettingsWidgetView newWidget, ISettingsWidgetController newWidgetController, SettingsWidgetModel widgetConfig);
    }

    /// <summary>
    /// This controller is in charge of manage all the logic related to a SECTION.
    /// </summary>
    [CreateAssetMenu(menuName = "Settings/Controllers/Section", fileName = "SettingsSectionController")]
    public class SettingsSectionController : ScriptableObject, ISettingsSectionController
    {
        public List<ISettingsWidgetView> widgets { get; } = new List<ISettingsWidgetView>();

        public void AddWidget(
            ISettingsWidgetView newWidget,
            ISettingsWidgetController newWidgetController,
            SettingsWidgetModel widgetConfig)
        {
            newWidget.Initialize(widgetConfig.title, newWidgetController, widgetConfig.controlColumns.ToList());
            widgets.Add(newWidget);
        }
    }
}