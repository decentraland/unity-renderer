using DCL.SettingsPanelHUD.Common;
using DCL.SettingsPanelHUD.Sections;
using System.Collections.Generic;
using System.Linq;

namespace DCL.SettingsPanelHUD
{
    /// <summary>
    /// Interface to implement a controller for the main settings panel.
    /// </summary>
    public interface ISettingsPanelHUDController
    {
        /// <summary>
        /// List of all SECTIONS added to the main settings panel.
        /// </summary>
        List<ISettingsSectionView> sections { get; }

        /// <summary>
        /// All the needed logic to initializes the controller and its associated view, and make them works.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Adds a SECTION into the main settings panel.
        /// </summary>
        /// <param name="newMenuButton">MonoBehaviour that will represent the menu button associated to the new SECTION.</param>
        /// <param name="newSection">New SECTION that will be added.</param>
        /// <param name="newSectionController">Controller belonging to the new SECTION.</param>
        /// <param name="sectionConfig">Model that will contain the configuration of the new SECTION.</param>
        void AddSection(SettingsButtonEntry newMenuButton, ISettingsSectionView newSection, ISettingsSectionController newSectionController, SettingsSectionModel sectionConfig);

        /// <summary>
        /// Opens a specific SECTION of the main settings panel.
        /// </summary>
        /// <param name="sectionToOpen">SECTION to be opened.</param>
        void OpenSection(ISettingsSectionView sectionToOpen);

        /// <summary>
        /// Opens a specific SECTION of the main settings panel.
        /// </summary>
        /// <param name="sectionIndex">SECTION index to be opened. The index must be less than the section list length.</param>
        void OpenSection(int sectionIndex);

        /// <summary>
        /// Mark a specific menu button as selected.
        /// </summary>
        /// <param name="buttonIndex">Menu button index to be selected.</param>
        void MarkMenuButtonAsSelected(int buttonIndex);

        /// <summary>
        /// Save all the current settings values.
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Reset all the current settings to their default values.
        /// </summary>
        void ResetAllSettings();
    }

    /// <summary>
    /// This controller is in charge of manage all the logic related to the main settings panel.
    /// </summary>
    public class SettingsPanelHUDController : IHUD, ISettingsPanelHUDController
    {
        public SettingsPanelHUDView view { get; private set; }

        public event System.Action OnOpen;
        public event System.Action OnClose;

        public List<ISettingsSectionView> sections { get; } = new List<ISettingsSectionView>();

        private List<SettingsButtonEntry> menuButtons = new List<SettingsButtonEntry>();

        public SettingsPanelHUDController()
        {
            view = SettingsPanelHUDView.Create();
        }

        public void Dispose()
        {
            if (view != null)
                UnityEngine.Object.Destroy(view.gameObject);
        }

        public void SetVisibility(bool visible)
        {
            if (!visible && view.isOpen)
            {
                OnClose?.Invoke();
            }
            else if (visible && !view.isOpen)
            {
                OnOpen?.Invoke();
            }

            view.SetVisibility(visible);
        }

        public void Initialize()
        {
            view.Initialize(this, this);
        }

        public void AddSection(
            SettingsButtonEntry newMenuButton,
            ISettingsSectionView newSection,
            ISettingsSectionController newSectionController,
            SettingsSectionModel sectionConfig)
        {
            newMenuButton?.Initialize(sectionConfig.icon, sectionConfig.text);

            newSection.Initialize(newSectionController, sectionConfig.widgets.ToList());
            newSection.SetActive(false);
            sections.Add(newSection);

            newMenuButton?.ConfigureAction(() =>
            {
                foreach (var button in menuButtons)
                {
                    button.MarkAsSelected(false);
                }
                newMenuButton.MarkAsSelected(true);

                OpenSection(newSection);
            });

            menuButtons.Add(newMenuButton);
        }

        public void OpenSection(ISettingsSectionView sectionToOpen)
        {
            foreach (var section in sections)
            {
                if (section == sectionToOpen)
                    continue;

                section.SetActive(false);
            }

            sectionToOpen.SetActive(true);
        }

        public void OpenSection(int sectionIndex)
        {
            foreach (var section in sections)
            {
                if (section == sections[sectionIndex])
                    continue;

                section.SetActive(false);
            }

            sections[sectionIndex].SetActive(true);
        }

        public void MarkMenuButtonAsSelected(int buttonIndex)
        {
            foreach (var button in menuButtons)
            {
                button.MarkAsSelected(false);
            }

            menuButtons[buttonIndex].MarkAsSelected(true);
        }

        public void SaveSettings()
        {
            Settings.i.SaveSettings();
        }

        public void ResetAllSettings()
        {
            Settings.i.ResetAllSettings();
        }
    }
}