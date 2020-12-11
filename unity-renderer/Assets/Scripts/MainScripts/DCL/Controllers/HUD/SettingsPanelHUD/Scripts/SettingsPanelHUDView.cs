using DCL.Helpers;
using DCL.SettingsPanelHUD.Sections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD
{
    /// <summary>
    /// MonoBehaviour that represents the main settings panel view and will act as a factory of SECTIONS.
    /// </summary>
    public class SettingsPanelHUDView : MonoBehaviour
    {
        [Header("General configuration")]
        [SerializeField] private GameObject mainWindow;

        [Header("Sections configuration")]
        [SerializeField] private SettingsPanelModel settingsPanelConfig;
        [SerializeField] private Transform menuButtonsContainer;
        [SerializeField] private Transform sectionsContainer;

        [Header("Reset All configuration")]
        [SerializeField] private Button resetAllButton;
        [SerializeField] private ShowHideAnimator resetAllConfirmation;
        [SerializeField] private Button resetAllOkButton;
        [SerializeField] private Button resetAllCancelButton;
        [SerializeField] private GameObject resetAllBlackOverlay;

        [Header("Open/Close Settings")]
        [SerializeField] private Button closeButton;
        [SerializeField] private InputAction_Trigger closeAction;
        [SerializeField] private InputAction_Trigger openAction;

        [Header("Animations")]
        [SerializeField] private ShowHideAnimator settingsAnimator;

        public bool isOpen { get; private set; }

        private const string PATH = "SettingsPanelHUD";

        private IHUD hudController;
        private ISettingsPanelHUDController settingsPanelController;

        public static SettingsPanelHUDView Create()
        {
            SettingsPanelHUDView view = Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<SettingsPanelHUDView>();
            view.name = "_SettingsPanelHUD";
            return view;
        }

        public void Initialize(IHUD hudController, ISettingsPanelHUDController settingsPanelController)
        {
            this.hudController = hudController;
            this.settingsPanelController = settingsPanelController;

            openAction.OnTriggered += OpenAction_OnTriggered;

            resetAllButton.onClick.AddListener(ShowResetAllConfirmation);
            resetAllCancelButton.onClick.AddListener(HideResetAllConfirmation);
            resetAllOkButton.onClick.AddListener(ResetAllSettings);

            closeButton.onClick.AddListener(CloseSettingsPanel);
            settingsAnimator.OnWillFinishHide += OnFinishHide;

            CreateSections();
            isOpen = !settingsAnimator.hideOnEnable;
            settingsAnimator.Hide(true);
        }

        public void Initialize(IHUD hudController, ISettingsPanelHUDController settingsPanelController, SettingsSectionList sections)
        {
            settingsPanelConfig.sections = sections;
            Initialize(hudController, settingsPanelController);
        }

        private void OnDestroy()
        {
            openAction.OnTriggered -= OpenAction_OnTriggered;

            if (settingsAnimator)
                settingsAnimator.OnWillFinishHide -= OnFinishHide;
        }

        private void CreateSections()
        {
            foreach (SettingsSectionModel sectionConfig in settingsPanelConfig.sections)
            {
                var newMenuButton = Instantiate(sectionConfig.menuButtonPrefab, menuButtonsContainer);
                var newSection = Instantiate(sectionConfig.sectionPrefab, sectionsContainer);
                newSection.gameObject.name = $"Section_{sectionConfig.text}";
                var newSectionController = Instantiate(sectionConfig.sectionController);
                settingsPanelController.AddSection(newMenuButton, newSection, newSectionController, sectionConfig);
            }

            settingsPanelController.OpenSection(0);
            settingsPanelController.MarkMenuButtonAsSelected(0);
        }

        private void ShowResetAllConfirmation()
        {
            resetAllConfirmation.Show();
            resetAllBlackOverlay.SetActive(true);
        }

        private void HideResetAllConfirmation()
        {
            resetAllConfirmation.Hide();
            resetAllBlackOverlay.SetActive(false);
        }

        private void ResetAllSettings()
        {
            settingsPanelController.ResetAllSettings();
            resetAllConfirmation.Hide();
            resetAllBlackOverlay.SetActive(false);
        }

        private void CloseSettingsPanel()
        {
            hudController.SetVisibility(false);
        }

        public void SetVisibility(bool visible)
        {
            if (visible && !isOpen)
                AudioScriptableObjects.dialogOpen.Play(true);
            else if (isOpen)
                AudioScriptableObjects.dialogClose.Play(true);

            closeAction.OnTriggered -= CloseAction_OnTriggered;
            if (visible)
            {
                closeAction.OnTriggered += CloseAction_OnTriggered;
                settingsAnimator.Show();
                mainWindow.SetActive(true);
                HideResetAllConfirmation();
            }
            else
            {
                settingsAnimator.Hide();
                settingsPanelController.SaveSettings();
            }

            isOpen = visible;
        }

        private void OpenAction_OnTriggered(DCLAction_Trigger action)
        {
            Utils.UnlockCursor();
            hudController.SetVisibility(!isOpen);
        }

        private void CloseAction_OnTriggered(DCLAction_Trigger action)
        {
            CloseSettingsPanel();
        }

        private void OnFinishHide(ShowHideAnimator animator)
        {
            mainWindow.SetActive(false);
        }
    }
}