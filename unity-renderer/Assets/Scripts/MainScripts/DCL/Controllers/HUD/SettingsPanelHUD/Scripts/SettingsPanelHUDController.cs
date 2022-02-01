using DCL.SettingsPanelHUD.Sections;
using System.Collections.Generic;
using System.Linq;
using DCL.SettingsCommon;
using UnityEngine;
using DCL.HelpAndSupportHUD;

namespace DCL.SettingsPanelHUD
{
    /// <summary>
    /// Interface to implement a controller for the main settings panel.
    /// </summary>
    public interface ISettingsPanelHUDController
    {
        event System.Action OnRestartTutorial;

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

        /// <summary>
        /// Set the tutorial button as enabled/disabled.
        /// </summary>
        /// <param name="isEnabled">True for set it as enabled.</param>
        void SetTutorialButtonEnabled(bool isEnabled);
    }

    /// <summary>
    /// This controller is in charge of manage all the logic related to the main settings panel.
    /// </summary>
    public class SettingsPanelHUDController : IHUD, ISettingsPanelHUDController
    {
        private const string SECTION_TO_ACTIVATE_WORLD_PREVIEW = "graphics";

        public SettingsPanelHUDView view { get; private set; }

        public event System.Action OnOpen;
        public event System.Action OnClose;

        public List<ISettingsSectionView> sections { get; } = new List<ISettingsSectionView>();

        private List<SettingsButtonEntry> menuButtons = new List<SettingsButtonEntry>();
        private HelpAndSupportHUDController helpAndSupportHud;

        BaseVariable<bool> settingsPanelVisible => DataStore.i.settings.settingsPanelVisible;
        BaseVariable<Transform> configureSettingsInFullscreenMenu => DataStore.i.exploreV2.configureSettingsInFullscreenMenu;

        public event System.Action OnRestartTutorial;

        public void Initialize()
        {
            view = CreateView();
            view.Initialize(this, this);

            view.OnRestartTutorial += () =>
            {
                OnRestartTutorial?.Invoke();
                DataStore.i.exploreV2.isOpen.Set(false);
            };

            view.OnHelpAndSupportClicked += () => helpAndSupportHud.SetVisibility(true);

            settingsPanelVisible.OnChange += OnSettingsPanelVisibleChanged;
            OnSettingsPanelVisibleChanged(settingsPanelVisible.Get(), false);

            configureSettingsInFullscreenMenu.OnChange += ConfigureSettingsInFullscreenMenuChanged;
            ConfigureSettingsInFullscreenMenuChanged(configureSettingsInFullscreenMenu.Get(), null);

            DataStore.i.settings.isInitialized.Set(true);
        }
        protected virtual SettingsPanelHUDView CreateView() { return SettingsPanelHUDView.Create(); }

        public void Dispose()
        {
            settingsPanelVisible.OnChange -= OnSettingsPanelVisibleChanged;
            configureSettingsInFullscreenMenu.OnChange -= ConfigureSettingsInFullscreenMenuChanged;

            if (view != null)
                UnityEngine.Object.Destroy(view.gameObject);
        }

        public void SetVisibility(bool visible) { settingsPanelVisible.Set(visible); }

        private void OnSettingsPanelVisibleChanged(bool current, bool previous) { SetVisibility_Internal(current); }

        public void SetVisibility_Internal(bool visible)
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

        public void AddSection(
            SettingsButtonEntry newMenuButton,
            ISettingsSectionView newSection,
            ISettingsSectionController newSectionController,
            SettingsSectionModel sectionConfig)
        {
            newMenuButton?.Initialize(sectionConfig.icon, sectionConfig.text);

            newSection.Initialize(newSectionController, sectionConfig.widgets.ToList(), sectionConfig.text);
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
                section.SetActive(section == sectionToOpen);

                if (section == sectionToOpen)
                    view.SetWorldPreviewActive(section.sectionName.ToLower() == SECTION_TO_ACTIVATE_WORLD_PREVIEW);
            }
        }

        public void OpenSection(int sectionIndex)
        {
            for (int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                section.SetActive(i == sectionIndex);

                if (i == sectionIndex)
                    view.SetWorldPreviewActive(section.sectionName.ToLower() == SECTION_TO_ACTIVATE_WORLD_PREVIEW);
            }
        }

        public void MarkMenuButtonAsSelected(int buttonIndex)
        {
            for (int i = 0; i < menuButtons.Count; i++)
            {
                var button = menuButtons[i];
                button.MarkAsSelected(i == buttonIndex);
            }
        }

        public virtual void SaveSettings() { Settings.i.SaveSettings(); }

        public void ResetAllSettings() { Settings.i.ResetAllSettings(); }

        public void SetTutorialButtonEnabled(bool isEnabled)
        {
            if (view != null)
                view.SetTutorialButtonEnabled(isEnabled);
        }

        public void AddHelpAndSupportWindow(HelpAndSupportHUDController controller)
        {
            if (controller == null || controller.view == null)
            {
                Debug.LogWarning("AddHelpAndSupportWindow >>> Help and Support window doesn't exist yet!");
                return;
            }

            helpAndSupportHud = controller;
            view.OnAddHelpAndSupportWindow();
        }

        private void ConfigureSettingsInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) { view.SetAsFullScreenMenuMode(currentParentTransform); }
    }
}