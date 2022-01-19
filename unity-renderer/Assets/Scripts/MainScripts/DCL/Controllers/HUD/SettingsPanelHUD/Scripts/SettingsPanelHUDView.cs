using DCL.Helpers;
using DCL.SettingsPanelHUD.Sections;
using System.Collections;
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
        [SerializeField] private Button backgroundButton;
        [SerializeField] private InputAction_Trigger openAction;

        [Header("World Preview Window")]
        [SerializeField] private RectTransform worldPreviewWindowTransform;
        [SerializeField] private CanvasGroup worldPreviewCanvasGroup;
        [SerializeField] private RawImage worldPreviewRawImage;

        [Header("Others")]
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button reportBugButton;
        [SerializeField] private Button helpAndSupportButton;

        [Header("Animations")]
        [SerializeField] private ShowHideAnimator settingsAnimator;

        public bool isOpen { get; private set; }

        private const string PATH = "SettingsPanelHUD";
        private const float WORLD_PREVIEW_MIN_WIDTH_TO_BE_SHOWED = 200f;

        private IHUD hudController;
        private ISettingsPanelHUDController settingsPanelController;
        private Vector2 originalWorldPreviewWindowSize;

        public event System.Action OnRestartTutorial;
        public event System.Action OnHelpAndSupportClicked;

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
            backgroundButton.onClick.AddListener(CloseSettingsPanel);
            settingsAnimator.OnWillFinishHide += OnFinishHide;

            CreateSections();
            isOpen = !settingsAnimator.hideOnEnable;
            settingsAnimator.Hide(true);

            tutorialButton.onClick.AddListener(() => OnRestartTutorial?.Invoke());

            originalWorldPreviewWindowSize = worldPreviewWindowTransform.rect.size;
            DataStore.i.screen.size.OnChange += ScreenSizeChanged;
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

            tutorialButton.onClick.RemoveAllListeners();
            helpAndSupportButton.onClick.RemoveAllListeners();

            DataStore.i.screen.size.OnChange -= ScreenSizeChanged;
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
            DataStore.i.exploreV2.isSomeModalOpen.Set(true);
            resetAllConfirmation.Show();
            resetAllBlackOverlay.SetActive(true);
        }

        private void HideResetAllConfirmation()
        {
            DataStore.i.exploreV2.isSomeModalOpen.Set(false);
            resetAllConfirmation.Hide();
            resetAllBlackOverlay.SetActive(false);
        }

        private void ResetAllSettings()
        {
            DataStore.i.exploreV2.isSomeModalOpen.Set(false);
            settingsPanelController.ResetAllSettings();
            resetAllConfirmation.Hide();
            resetAllBlackOverlay.SetActive(false);
        }

        private void CloseSettingsPanel() { hudController.SetVisibility(false); }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                settingsAnimator.Show();
                mainWindow.SetActive(true);
                HideResetAllConfirmation();
                settingsPanelController.OpenSection(0);
                settingsPanelController.MarkMenuButtonAsSelected(0);
            }
            else
            {
                settingsAnimator.Hide();
                settingsPanelController.SaveSettings();
                SetWorldPreviewActive(false);
            }
            
            isOpen = visible;
        }

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            transform.SetParent(parentTransform);
            transform.localScale = Vector3.one;

            RectTransform rectTransform = transform as RectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        public void SetTutorialButtonEnabled(bool isEnabled) { tutorialButton.enabled = isEnabled; }

        public void OnAddHelpAndSupportWindow()
        {
            helpAndSupportButton.gameObject.SetActive(true);
            helpAndSupportButton.onClick.AddListener(() => OnHelpAndSupportClicked?.Invoke());
        }

        public void SetWorldPreviewActive(bool isActive)
        {
            worldPreviewWindowTransform.gameObject.SetActive(isActive);
            DataStore.i.camera.outputTexture.Set(isActive ? worldPreviewRawImage.texture as RenderTexture : null);
            CommonScriptableObjects.isFullscreenHUDOpen.Set(DataStore.i.exploreV2.isOpen.Get() && !isActive);
        }

        private void OpenAction_OnTriggered(DCLAction_Trigger action)
        {
            Utils.UnlockCursor();
            hudController.SetVisibility(!isOpen);
        }

        private void OnFinishHide(ShowHideAnimator animator) { mainWindow.SetActive(false); }

        private void ScreenSizeChanged(Vector2Int current, Vector2Int previous)
        {
            StartCoroutine(RefreshWorldPreviewSize());
        }

        private IEnumerator RefreshWorldPreviewSize()
        {
            yield return null;

            float newHeight = worldPreviewWindowTransform.rect.size.x * originalWorldPreviewWindowSize.y / originalWorldPreviewWindowSize.x;

            worldPreviewWindowTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                newHeight);

            worldPreviewCanvasGroup.alpha = worldPreviewWindowTransform.rect.size.x >= WORLD_PREVIEW_MIN_WIDTH_TO_BE_SHOWED ? 1f : 0f;

            Utils.ForceRebuildLayoutImmediate(sectionsContainer as RectTransform);
        }
    }
}