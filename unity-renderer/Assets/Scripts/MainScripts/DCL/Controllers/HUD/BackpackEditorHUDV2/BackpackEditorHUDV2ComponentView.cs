using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentView : BaseComponentView<BackpackEditorHUDModel>, IBackpackEditorHUDView
    {
        private const int AVATAR_SECTION_INDEX = 0;
        private const int EMOTES_SECTION_INDEX = 1;

        [SerializeField] private SectionSelectorComponentView sectionSelector;
        [SerializeField] private GameObject wearablesSection;
        [SerializeField] private GameObject emotesSection;
        [SerializeField] private BackpackPreviewPanel backpackPreviewPanel;

        public override bool isVisible => gameObject.activeInHierarchy;
        public Transform EmotesSectionTransform => emotesSection.transform;

        private Transform thisTransform;
        private bool isAvatarDirty;
        private AvatarModel avatarModelToUpdate;

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
        }

        private void Update() =>
            UpdateAvatarModelWhenNeeded();

        public override void Dispose()
        {
            base.Dispose();

            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.RemoveAllListeners();
            backpackPreviewPanel.Dispose();
        }

        public static IBackpackEditorHUDView Create() =>
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
        }

        public override void RefreshControl() { }

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
            async UniTaskVoid UpdateAvatarAsync()
            {
                await backpackPreviewPanel.TryUpdatePreviewModelAsync(avatarModelToUpdate);
                backpackPreviewPanel.SetLoadingActive(false);
            }

            if (!isAvatarDirty)
                return;

            UpdateAvatarAsync().Forget();
            isAvatarDirty = false;
        }
    }
}
