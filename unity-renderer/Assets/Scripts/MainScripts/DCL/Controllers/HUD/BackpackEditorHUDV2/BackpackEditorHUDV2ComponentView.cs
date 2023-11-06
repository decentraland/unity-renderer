using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DG.Tweening;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentView : BaseComponentView<BackpackEditorHUDModel>, IBackpackEditorHUDView, IPointerDownHandler
    {
        private const string SIGN_UP_HEADER_TITLE_FOR_FISRT_STEP = "Customize Your Avatar";
        private const string SIGN_UP_HEADER_TITLE_FOR_SECOND_STEP = "Final Details";

        public event Action<Color> OnColorChanged;
        public event Action OnColorPickerToggle;
        public event Action OnContinueSignup;
        public event Action OnAvatarUpdated;
        public event Action OnOutfitsOpened;
        public event Action OnVRMExport;
        public event Action<SignUpStage> OnSignUpBackClicked;

        private const int AVATAR_SECTION_INDEX = 0;
        private const int EMOTES_SECTION_INDEX = 1;
        private const int MS_TO_RESET_PREVIEW_ANIMATION = 200;

        [SerializeField] internal SectionSelectorComponentView sectionSelector;
        [SerializeField] internal GameObject wearablesSection;
        [SerializeField] internal CanvasGroup wearablesSectionCanvasGroup;
        [SerializeField] internal Image wearablesSectionBackground;
        [SerializeField] internal GameObject emotesSection;
        [SerializeField] private BackpackPreviewPanel backpackPreviewPanel;
        [SerializeField] private WearableGridComponentView wearableGridComponentView;
        [SerializeField] private AvatarSlotsView avatarSlotsView;
        [SerializeField] internal ColorPickerComponentView colorPickerComponentView;
        [SerializeField] internal ColorPresetsSO colorPresetsSO;
        [SerializeField] internal ColorPresetsSO skinColorPresetsSO;
        [SerializeField] internal Color selectedOutfitButtonColor;
        [SerializeField] private BackpackFiltersComponentView backpackFiltersComponentView;
        [SerializeField] private OutfitsSectionComponentView outfitsSectionComponentView;
        [SerializeField] internal Button saveAvatarButton;
        [SerializeField] internal GameObject normalSection;
        [SerializeField] internal GameObject outfitSection;
        [SerializeField] internal Button outfitButton;
        [SerializeField] internal Image outfitButtonIcon;
        [SerializeField] internal Button vrmExportButton;
        [SerializeField] internal RectTransform vrmExportedToast;
        [SerializeField] internal GameObject background;
        [SerializeField] internal GameObject hints;

        [Header("Sign Up Mode")]
        [SerializeField] internal GameObject signUpHeader;
        [SerializeField] internal TMP_Text signUpHeaderTitle;
        [SerializeField] internal GameObject backgroundForSignUp;
        [SerializeField] internal Button backButton;
        [SerializeField] internal Button nextButton;
        [SerializeField] internal GameObject[] objectsToDeactivateInSignUpMode;

        [Header("SignUp Mode Transitions")]
        [SerializeField] internal RectTransform wearablesBackgroundForSignUp;
        [SerializeField] internal CanvasGroup wearablesBackgroundForSignUpCanvasGroup;
        [SerializeField] internal Ease transitionEase = Ease.InOutExpo;
        [SerializeField] internal float transitionDuration = 0.5f;
        [SerializeField] internal float transitionDistance = 1800f;

        public IReadOnlyList<SkinnedMeshRenderer> originalVisibleRenderers => backpackPreviewPanel?.originalVisibleRenderers;
        public IAvatarEmotesController EmotesController => backpackPreviewPanel?.EmotesController;
        public override bool isVisible => gameObject.activeInHierarchy;
        public Transform EmotesSectionTransform => emotesSection.transform;
        public WearableGridComponentView WearableGridComponentView => wearableGridComponentView;
        public AvatarSlotsView AvatarSlotsView => avatarSlotsView;
        public BackpackFiltersComponentView BackpackFiltersComponentView => backpackFiltersComponentView;
        public OutfitsSectionComponentView OutfitsSectionComponentView => outfitsSectionComponentView;
        private DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;

        private Transform thisTransform;
        private bool isAvatarDirty;
        private AvatarModel avatarModelToUpdate;
        private CancellationTokenSource updateAvatarCts = new ();
        private CancellationTokenSource snapshotsCts = new ();
        private SignUpStage currentStage;
        private Vector2 originalAnchorPositionOfWearablesBackgroundForSignUp;
        private Vector2 originalAnchorPositionOfWearablesSection;

        public override void Awake()
        {
            base.Awake();

            thisTransform = transform;
            backpackPreviewPanel.SetLoadingActive(false);
            originalAnchorPositionOfWearablesBackgroundForSignUp = wearablesBackgroundForSignUp.anchoredPosition;
            originalAnchorPositionOfWearablesSection = ((RectTransform)wearablesSection.transform).anchoredPosition;
            saveAvatarButton.onClick.AddListener(() => OnContinueSignup?.Invoke());
            nextButton.onClick.AddListener(() => OnContinueSignup?.Invoke());
            backButton.onClick.AddListener(() => OnSignUpBackClicked?.Invoke(currentStage));
        }

        public void Initialize(
            ICharacterPreviewFactory characterPreviewFactory,
            IPreviewCameraRotationController avatarPreviewRotationController,
            IPreviewCameraPanningController avatarPreviewPanningController,
            IPreviewCameraZoomController avatarPreviewZoomController)
        {
            ConfigureSectionSelector();

            backpackPreviewPanel.Initialize(
                characterPreviewFactory,
                avatarPreviewRotationController,
                avatarPreviewPanningController,
                avatarPreviewZoomController);

            colorPickerComponentView.OnColorChanged += OnColorPickerColorChanged;
            colorPickerComponentView.OnColorPickerToggle += ColorPickerToggle;

            outfitButton.onClick.RemoveAllListeners();
            outfitButton.onClick.AddListener(ToggleOutfitSection);

            vrmExportButton.onClick.RemoveAllListeners();
            vrmExportButton.onClick.AddListener(() => OnVRMExport?.Invoke());

            outfitsSectionComponentView.OnBackButtonPressed += ToggleNormalSection;
        }

        public void SetOutfitsEnabled(bool isEnabled) =>
            outfitButton.gameObject.SetActive(isEnabled);

        private void ToggleOutfitSection()
        {
            if (outfitSection.activeInHierarchy)
                ToggleNormalSection();
            else
            {
                normalSection.SetActive(false);
                outfitSection.SetActive(true);
                outfitButton.image.color = selectedOutfitButtonColor;
                outfitButtonIcon.color = Color.white;
                OnOutfitsOpened?.Invoke();
            }
        }

        private void ToggleNormalSection()
        {
            normalSection.SetActive(true);
            outfitSection.SetActive(false);
            outfitButton.image.color = Color.white;
            outfitButtonIcon.color = Color.black;
        }

        private void Update() =>
            UpdateAvatarModelWhenNeeded();

        public override void Dispose()
        {
            base.Dispose();

            updateAvatarCts.SafeCancelAndDispose();
            updateAvatarCts = null;

            snapshotsCts.SafeCancelAndDispose();
            snapshotsCts = null;

            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.RemoveAllListeners();
            emotesCustomizationDataStore.isEmotesCustomizationSelected.OnChange -= OnEmotesCustomizationSelected;
            backpackPreviewPanel.Dispose();

            colorPickerComponentView.OnColorChanged -= OnColorPickerColorChanged;
            colorPickerComponentView.OnColorPickerToggle -= ColorPickerToggle;
            outfitsSectionComponentView.OnBackButtonPressed -= ToggleNormalSection;
        }

        public static BackpackEditorHUDV2ComponentView Create() =>
            Instantiate(Resources.Load<BackpackEditorHUDV2ComponentView>("BackpackEditorHUDV2"));

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            backpackPreviewPanel.SetPreviewEnabled(true);
        }

        public override void Hide(bool instant = false)
        {
            ToggleNormalSection();
            gameObject.SetActive(false);
            backpackPreviewPanel.SetPreviewEnabled(false);
            colorPickerComponentView.SetActive(false);
        }

        public override void RefreshControl() { }

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            thisTransform.SetParent(parentTransform);
            thisTransform.localScale = Vector3.one;

            RectTransform rectTransform = thisTransform as RectTransform;
            if (rectTransform == null) return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        public void PlayPreviewEmote(string emoteId) =>
            backpackPreviewPanel.PlayPreviewEmote(emoteId);

        public void PlayPreviewEmote(string emoteId, long timestamp)
        {
            if (avatarModelToUpdate != null)
                avatarModelToUpdate.expressionTriggerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            backpackPreviewPanel.PlayPreviewEmote(emoteId, timestamp);
        }

        public void UpdateAvatarPreview(AvatarModel avatarModel)
        {
            if (avatarModel?.wearables == null)
                return;

            // We delay the updating of the avatar 1 frame to disengage from the kernel message flow
            // otherwise the cancellation of the updating task throws an exception that is catch by
            // kernel setthrew method, which floods the analytics.
            // Also it updates just once if its called many times in a row
            isAvatarDirty = true;
            avatarModelToUpdate = avatarModel;

            backpackPreviewPanel.SetLoadingActive(true);
        }

        public void SetAvatarPreviewFocus(PreviewCameraFocus focus, bool useTransition = true) =>
            backpackPreviewPanel.SetFocus(focus, useTransition);

        public void TakeSnapshotsAfterStopPreviewAnimation(IBackpackEditorHUDView.OnSnapshotsReady onSuccess, Action onFailed)
        {
            async UniTaskVoid TakeSnapshotsAfterStopPreviewAnimationAsync(CancellationToken ct)
            {
                ResetPreviewPanel();
                await UniTask.Delay(MS_TO_RESET_PREVIEW_ANIMATION, cancellationToken: ct);

                backpackPreviewPanel.TakeSnapshots(
                    (face256, body) => onSuccess?.Invoke(face256, body),
                    () => onFailed?.Invoke());
            }

            snapshotsCts = snapshotsCts.SafeRestart();
            TakeSnapshotsAfterStopPreviewAnimationAsync(snapshotsCts.Token).Forget();
        }

        public void SetColorPickerVisibility(bool isActive) =>
            colorPickerComponentView.gameObject.SetActive(isActive);

        public void SetColorPickerAsSkinMode(bool isSkinMode)
        {
            colorPickerComponentView.SetShowOnlyPresetColors(isSkinMode);
            colorPickerComponentView.SetColorList(isSkinMode ? skinColorPresetsSO.colors : colorPresetsSO.colors);
        }

        public void UpdateHideUnhideStatus(string slotCategory, HashSet<string> forceRender) =>
            avatarSlotsView.SetHideUnhideStatus(slotCategory, forceRender.Contains(slotCategory));

        public void SetColorPickerValue(Color color)
        {
            colorPickerComponentView.SetColorSelector(color);
            colorPickerComponentView.UpdateSliderValues(color);
        }

        public void ShowContinueSignup() =>
            saveAvatarButton.gameObject.SetActive(true);

        public void HideContinueSignup() =>
            saveAvatarButton.gameObject.SetActive(false);

        public void SetVRMButtonActive(bool enabled)
        {
            vrmExportButton.gameObject.SetActive(enabled);
        }

        public void SetVRMButtonEnabled(bool enabled)
        {
            vrmExportButton.enabled = enabled;
        }

        public void SetVRMSuccessToastActive(bool active)
        {
            vrmExportedToast.gameObject.SetActive(active);
        }

        public void SetSignUpModeActive(bool isActive)
        {
            signUpHeader.SetActive(isActive);
            backgroundForSignUp.SetActive(isActive);
            background.SetActive(!isActive);
            wearablesSectionBackground.enabled = !isActive;

            foreach (GameObject go in objectsToDeactivateInSignUpMode)
                go.SetActive(!isActive);
        }

        public void SetSignUpStage(SignUpStage stage)
        {
            currentStage = stage;
            nextButton.gameObject.SetActive(stage == SignUpStage.CustomizeAvatar);
            signUpHeaderTitle.text = stage == SignUpStage.CustomizeAvatar ? SIGN_UP_HEADER_TITLE_FOR_FISRT_STEP : SIGN_UP_HEADER_TITLE_FOR_SECOND_STEP;
            hints.SetActive(stage == SignUpStage.CustomizeAvatar);
            PlayTransitionAnimation(stage);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerPressRaycast.gameObject != colorPickerComponentView.gameObject)
                colorPickerComponentView.SetActive(false);
        }

        private void ConfigureSectionSelector()
        {
            sectionSelector.GetSection(AVATAR_SECTION_INDEX)
                           .onSelect.AddListener((isSelected) =>
                            {
                                wearablesSection.SetActive(isSelected);

                                if (isSelected)
                                    ResetPreviewPanel();
                            });

            sectionSelector.GetSection(EMOTES_SECTION_INDEX)
                           .onSelect.AddListener((isSelected) =>
                            {
                                emotesCustomizationDataStore.isEmotesCustomizationSelected.Set(isSelected);

                                emotesSection.SetActive(isSelected);

                                if (isSelected)
                                    ResetPreviewPanel();
                            });

            emotesCustomizationDataStore.isEmotesCustomizationSelected.OnChange += OnEmotesCustomizationSelected;
        }

        private void OnEmotesCustomizationSelected(bool current, bool previous)
        {
            if (current)
                sectionSelector.GetSection(EMOTES_SECTION_INDEX).SelectToggle();
        }

        public void ResetPreviewPanel()
        {
            backpackPreviewPanel.ResetPreviewEmote();
            backpackPreviewPanel.ResetPreviewRotation();
            SetAvatarPreviewFocus(PreviewCameraFocus.DefaultEditing, false);
        }

        private void UpdateAvatarModelWhenNeeded()
        {
            async UniTaskVoid UpdateAvatarAsync(CancellationToken ct)
            {
                try
                {
                    await backpackPreviewPanel.TryUpdatePreviewModelAsync(avatarModelToUpdate, ct);
                    backpackPreviewPanel.SetLoadingActive(false);
                    OnAvatarUpdated?.Invoke();
                }
                catch (OperationCanceledException) { Debug.LogWarning("Update avatar preview cancelled"); }
                catch (Exception e) { Debug.LogException(e); }
            }

            if (!isAvatarDirty)
                return;

            updateAvatarCts = updateAvatarCts.SafeRestart();
            UpdateAvatarAsync(updateAvatarCts.Token).Forget();
            isAvatarDirty = false;
        }

        private void OnColorPickerColorChanged(Color newColor) =>
            OnColorChanged?.Invoke(newColor);

        private void ColorPickerToggle() =>
            OnColorPickerToggle?.Invoke();

        private void PlayTransitionAnimation(SignUpStage stage)
        {
            Vector2 wearablesBackgroundForSignUpEndPosition = originalAnchorPositionOfWearablesBackgroundForSignUp;
            if (stage == SignUpStage.SetNameAndEmail)
                wearablesBackgroundForSignUpEndPosition.x += transitionDistance;
            wearablesBackgroundForSignUp.DOAnchorPos(wearablesBackgroundForSignUpEndPosition, transitionDuration).SetEase(transitionEase);
            wearablesBackgroundForSignUpCanvasGroup.DOFade(stage == SignUpStage.CustomizeAvatar ? 1f : 0f, transitionDuration).SetEase(transitionEase);
            wearablesBackgroundForSignUpCanvasGroup.blocksRaycasts = stage == SignUpStage.CustomizeAvatar;

            Vector2 wearablesSectionEndPosition = originalAnchorPositionOfWearablesSection;
            if (stage == SignUpStage.SetNameAndEmail)
                wearablesSectionEndPosition.x += transitionDistance;
            (wearablesSection.transform as RectTransform).DOAnchorPos(wearablesSectionEndPosition, transitionDuration).SetEase(transitionEase);
            wearablesSectionCanvasGroup.DOFade(stage == SignUpStage.CustomizeAvatar ? 1f : 0f, transitionDuration).SetEase(transitionEase);
            wearablesSectionCanvasGroup.blocksRaycasts = stage == SignUpStage.CustomizeAvatar;
        }
    }
}
