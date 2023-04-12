using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentView : BaseComponentView<BackpackEditorHUDModel>, IBackpackEditorHUDView
    {
        private const int AVATAR_SECTION_INDEX = 0;
        private const int EMOTES_SECTION_INDEX = 1;
        private const string RESET_PREVIEW_ANIMATION = "Idle";

        [Header("Sections")]
        [SerializeField] private SectionSelectorComponentView sectionSelector;
        [SerializeField] private GameObject wearablesSection;
        [SerializeField] private GameObject emotesSection;

        [Header("Avatar Preview")]
        [SerializeField] private RectTransform avatarPreviewPanel;
        [SerializeField] private PreviewCameraRotation avatarPreviewRotation;
        [SerializeField] private RawImage avatarPreviewImage;
        [SerializeField] internal GameObject avatarPreviewLoadingSpinner;

        public override bool isVisible => gameObject.activeInHierarchy;
        public Transform EmotesSectionTransform => emotesSection.transform;
        public ICharacterPreviewController CharacterPreview { get; private set; }

        private Transform thisTransform;
        private bool isAvatarDirty;
        private AvatarModel avatarModelToUpdate;

        public override void Awake()
        {
            base.Awake();
            thisTransform = transform;
            avatarPreviewLoadingSpinner.SetActive(false);
        }

        public void Initialize(ICharacterPreviewFactory characterPreviewFactory)
        {
            CharacterPreview = characterPreviewFactory.Create(CharacterPreviewMode.WithoutHologram, (RenderTexture) avatarPreviewImage.texture, false);
            CharacterPreview.SetFocus(CharacterPreviewController.CameraFocus.DefaultEditing);
            avatarPreviewRotation.OnHorizontalRotation += OnAvatarPreviewRotation;

            ConfigureSectionSelector();
        }

        private void Update() =>
            UpdateAvatarModelWhenNeeded();

        public override void Dispose()
        {
            base.Dispose();

            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.RemoveAllListeners();
            CharacterPreview.Dispose();

            avatarPreviewRotation.OnHorizontalRotation -= OnAvatarPreviewRotation;
        }

        public static IBackpackEditorHUDView Create() =>
            Instantiate(Resources.Load<BackpackEditorHUDV2ComponentView>("BackpackEditorHUDV2"));

        public override void Show(bool instant = false)
        {
            CharacterPreview.SetEnabled(true);
            gameObject.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
            CharacterPreview.SetEnabled(false);
            gameObject.SetActive(false);
        }

        public override void RefreshControl()
        {
        }

        public void Show() =>
            Show(true);

        public void Hide() =>
            Hide(true);

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            thisTransform.SetParent(parentTransform);
            thisTransform.localScale = Vector3.one;

            RectTransform rectTransform = thisTransform as RectTransform;
            if (rectTransform == null)
                return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        public void PlayPreviewEmote(string emoteId) =>
            CharacterPreview.PlayEmote(emoteId, (long)Time.realtimeSinceStartup);

        public void ResetPreviewEmote() =>
            PlayPreviewEmote(RESET_PREVIEW_ANIMATION);

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

            avatarPreviewLoadingSpinner.SetActive(true);
        }

        private void ConfigureSectionSelector()
        {
            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                wearablesSection.SetActive(isSelected);
                AnchorAvatarPreviewPanel(false);

                if (isSelected)
                    ResetPreviewEmote();
            });
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                emotesSection.SetActive(isSelected);
                AnchorAvatarPreviewPanel(true);

                if (isSelected)
                    ResetPreviewEmote();
            });
        }

        private void AnchorAvatarPreviewPanel(bool anchorRight)
        {
            avatarPreviewPanel.pivot = new Vector2(anchorRight ? 1 : 0, avatarPreviewPanel.pivot.y);
            avatarPreviewPanel.anchorMin = new Vector2(anchorRight ? 1 : 0, avatarPreviewPanel.anchorMin.y);
            avatarPreviewPanel.anchorMax = new Vector2(anchorRight ? 1 : 0, avatarPreviewPanel.anchorMax.y);
            avatarPreviewPanel.offsetMin = new Vector2(anchorRight ? -avatarPreviewPanel.rect.width : 0, avatarPreviewPanel.offsetMin.y);
            avatarPreviewPanel.offsetMax = new Vector2(anchorRight ? 0 : avatarPreviewPanel.rect.width, avatarPreviewPanel.offsetMax.y);
        }

        private void OnAvatarPreviewRotation(float angularVelocity) =>
            CharacterPreview.Rotate(angularVelocity);

        private void UpdateAvatarModelWhenNeeded()
        {
            if (isAvatarDirty)
            {
                async UniTaskVoid UpdateAvatarAsync()
                {
                    await CharacterPreview.TryUpdateModelAsync(avatarModelToUpdate);
                    avatarPreviewLoadingSpinner.SetActive(false);
                }

                UpdateAvatarAsync().Forget();
                isAvatarDirty = false;
            }
        }
    }
}
