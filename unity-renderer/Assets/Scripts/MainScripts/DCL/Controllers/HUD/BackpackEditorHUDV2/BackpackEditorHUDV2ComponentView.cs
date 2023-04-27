using Cysharp.Threading.Tasks;
using DCL.Tasks;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using System.Threading;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentView : BaseComponentView<BackpackEditorHUDModel>, IBackpackEditorHUDView, IPointerDownHandler
    {
        public event Action<Color> OnColorChanged;

        private const int AVATAR_SECTION_INDEX = 0;
        private const int EMOTES_SECTION_INDEX = 1;
        private const int MS_TO_RESET_PREVIEW_ANIMATION = 200;

        [SerializeField] private SectionSelectorComponentView sectionSelector;
        [SerializeField] private GameObject wearablesSection;
        [SerializeField] private GameObject emotesSection;
        [SerializeField] private BackpackPreviewPanel backpackPreviewPanel;
        [SerializeField] private WearableGridComponentView wearableGridComponentView;
        [SerializeField] private AvatarSlotsView avatarSlotsView;
        [SerializeField] private ColorPickerComponentView colorPickerComponentView;
        [SerializeField] private ColorPresetsSO colorPresetsSO;

        public override bool isVisible => gameObject.activeInHierarchy;
        public Transform EmotesSectionTransform => emotesSection.transform;
        public WearableGridComponentView WearableGridComponentView => wearableGridComponentView;
        public AvatarSlotsView AvatarSlotsView => avatarSlotsView;

        private Transform thisTransform;
        private bool isAvatarDirty;
        private AvatarModel avatarModelToUpdate;
        private CancellationTokenSource updateAvatarCts = new ();
        private CancellationTokenSource snapshotsCts = new ();

        public override void Awake()
        {
            base.Awake();

            thisTransform = transform;
            backpackPreviewPanel.SetLoadingActive(false);
        }

        public void Initialize(ICharacterPreviewFactory characterPreviewFactory)
        {
            ConfigureSectionSelector();
            backpackPreviewPanel.Initialize(characterPreviewFactory);
            colorPickerComponentView.OnColorChanged += OnColorPickerColorChanged;
            colorPickerComponentView.SetColorList(colorPresetsSO.colors);
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
            backpackPreviewPanel.Dispose();

            colorPickerComponentView.OnColorChanged -= OnColorPickerColorChanged;
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

        public void ResetPreviewEmote() =>
            backpackPreviewPanel.ResetPreviewEmote();

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

        public void TakeSnapshotsAfterStopPreviewAnimation(IBackpackEditorHUDView.OnSnapshotsReady onSuccess, Action onFailed)
        {
            async UniTaskVoid TakeSnapshotsAfterStopPreviewAnimationAsync(CancellationToken ct)
            {
                ResetPreviewEmote();
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

        public void SetColorPickerValue(Color color)
        {
            colorPickerComponentView.SetColorSelector(color);
            colorPickerComponentView.UpdateSliderValues(color);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerPressRaycast.gameObject != colorPickerComponentView.gameObject)
                colorPickerComponentView.SetActive(false);
        }

        private void ConfigureSectionSelector()
        {
            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                wearablesSection.SetActive(isSelected);
                backpackPreviewPanel.AnchorPreviewPanel(false);

                if (isSelected)
                    ResetPreviewEmote();
            });
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                emotesSection.SetActive(isSelected);
                backpackPreviewPanel.AnchorPreviewPanel(true);

                if (isSelected)
                    ResetPreviewEmote();
            });
        }

        private void UpdateAvatarModelWhenNeeded()
        {
            async UniTaskVoid UpdateAvatarAsync(CancellationToken ct)
            {
                await backpackPreviewPanel.TryUpdatePreviewModelAsync(avatarModelToUpdate, ct);
                backpackPreviewPanel.SetLoadingActive(false);
            }

            if (!isAvatarDirty)
                return;

            updateAvatarCts = updateAvatarCts.SafeRestart();
            UpdateAvatarAsync(updateAvatarCts.Token).Forget();
            isAvatarDirty = false;
        }

        private void OnColorPickerColorChanged(Color newColor) =>
            OnColorChanged?.Invoke(newColor);
    }
}
