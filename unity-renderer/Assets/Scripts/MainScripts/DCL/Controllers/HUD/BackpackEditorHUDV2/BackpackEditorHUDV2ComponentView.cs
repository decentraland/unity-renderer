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
        [SerializeField] private RectTransform avatarPreviewRectTransform;

        public override bool isVisible => gameObject.activeInHierarchy;
        public Transform EmotesSectionTransform => emotesSection.transform;

        private Transform thisTransform;

        public override void Awake()
        {
            base.Awake();
            thisTransform = transform;
        }

        public void Initialize()
        {
            ConfigureSectionSelector();
        }

        public override void Dispose()
        {
            base.Dispose();

            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.RemoveAllListeners();
        }

        public static BackpackEditorHUDV2ComponentView Create() =>
            Instantiate(Resources.Load<BackpackEditorHUDV2ComponentView>("BackpackEditorHUDV2"));

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
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

        private void ConfigureSectionSelector()
        {
            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                wearablesSection.SetActive(isSelected);
                AnchorAvatarPreviewPanel(false);
            });
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                emotesSection.SetActive(isSelected);
                AnchorAvatarPreviewPanel(true);
            });
        }

        private void AnchorAvatarPreviewPanel(bool anchorRight)
        {
            avatarPreviewRectTransform.pivot = new Vector2(anchorRight ? 1 : 0, avatarPreviewRectTransform.pivot.y);
            avatarPreviewRectTransform.anchorMin = new Vector2(anchorRight ? 1 : 0, avatarPreviewRectTransform.anchorMin.y);
            avatarPreviewRectTransform.anchorMax = new Vector2(anchorRight ? 1 : 0, avatarPreviewRectTransform.anchorMax.y);
            avatarPreviewRectTransform.offsetMin = new Vector2(anchorRight ? -avatarPreviewRectTransform.rect.width : 0, avatarPreviewRectTransform.offsetMin.y);
            avatarPreviewRectTransform.offsetMax = new Vector2(anchorRight ? 0 : avatarPreviewRectTransform.rect.width, avatarPreviewRectTransform.offsetMax.y);
        }
    }
}
