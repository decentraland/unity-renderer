using DCL.SettingsPanelHUD.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    /// <summary>
    /// Interface to implement a view for a SECTION.
    /// </summary>
    public interface ISettingsSectionView
    {
        /// <summary>
        /// All the needed logic to initializes the SECTION view and put its WIDGETS factory into operation.
        /// </summary>
        /// <param name="settingsSectionController">Controller that will be associated to this view.</param>
        /// <param name="widgets">List of WIDGETS associated to this SECTION.</param>
        void Initialize(ISettingsSectionController settingsSectionController, List<SettingsWidgetModel> widgets);

        /// <summary>
        /// Activates/deactivates the SECTION.
        /// </summary>
        /// <param name="active">True for SECTION activation.</param>
        void SetActive(bool active);
    }

    /// <summary>
    /// MonoBehaviour that represents a SECTION view and will act as a factory of WIDGETS.
    /// </summary>
    public class SettingsSectionView : MonoBehaviour, ISettingsSectionView
    {
        [SerializeField] private Transform widgetsContainer;

        private ISettingsSectionController settingsSectionController;
        private List<SettingsWidgetModel> widgets;

        public void Initialize(ISettingsSectionController settingsSectionController, List<SettingsWidgetModel> widgets)
        {
            this.settingsSectionController = settingsSectionController;
            this.widgets = widgets;

            CreateWidgets();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void CreateWidgets()
        {
            foreach (SettingsWidgetModel widgetConfig in widgets)
            {
                var newWidget = Instantiate(widgetConfig.widgetPrefab, widgetsContainer);
                newWidget.gameObject.name = $"Widget_{widgetConfig.title}";
                var newWidgetController = Instantiate(widgetConfig.widgetController);
                settingsSectionController.AddWidget(newWidget, newWidgetController, widgetConfig);
            }
        }
    }
}