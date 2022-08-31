using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class ChatChannelComponentView : BaseComponentView, IChatChannelWindowView, IComponentModelConfig<PublicChatModel>,
        IPointerDownHandler
    {
        private const int MEMBERS_SECTION_WIDTH = 280;

        [SerializeField] internal Button closeButton;
        [SerializeField] internal Button backButton;
        [SerializeField] internal Button optionsButton;
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal ChatHUDView chatView;
        [SerializeField] internal PublicChatModel model;
        [SerializeField] internal CanvasGroup[] previewCanvasGroup;
        [SerializeField] internal Vector2 previewModeSize;
        [SerializeField] internal GameObject messagesLoading;
        [SerializeField] internal ScrollRect scroll;
        [SerializeField] internal GameObject oldMessagesLoadingContainer;
        [SerializeField] internal ChannelContextualMenu contextualMenu;
        [SerializeField] internal TMP_Text memberCountLabel;
        [SerializeField] internal RectTransform collapsableArea;
        [SerializeField] internal Button membersIconButton;
        [SerializeField] internal ButtonComponentView expandMembersListButton;
        [SerializeField] internal ButtonComponentView collapseMembersListButton;
        [SerializeField] internal ChannelMembersComponentView membersList;

        private Coroutine alphaRoutine;
        private Vector2 originalSize;
        private bool isPreviewActivated;
        private bool isMembersSectionOpen;
        private float collapsableAreaOriginalWidth;

        public event Action OnClose;
        public event Action<bool> OnFocused;
        public event Action OnBack;
        public event Action OnRequireMoreMessages;
        public event Action OnLeaveChannel;
        public event Action OnShowMembersList;
        public event Action OnHideMembersList;

        public bool IsActive => gameObject.activeInHierarchy;
        public IChatHUDComponentView ChatHUD => chatView;
        public IChannelMembersComponentView ChannelMembersHUD => membersList;
        public RectTransform Transform => (RectTransform) transform;
        public bool IsFocused => isFocused;

        public static ChatChannelComponentView Create()
        {
            return Instantiate(Resources.Load<ChatChannelComponentView>("SocialBarV1/ChatChannelHUD"));
        }

        public override void Awake()
        {
            base.Awake();
            originalSize = ((RectTransform) transform).sizeDelta;
            backButton.onClick.AddListener(() => OnBack?.Invoke());
            closeButton.onClick.AddListener(() => OnClose?.Invoke());
            contextualMenu.OnLeave += () => OnLeaveChannel?.Invoke();
            optionsButton.onClick.AddListener(ShowOptionsMenu);
            scroll.onValueChanged.AddListener(scrollPos =>
            {
                if (isPreviewActivated) return;

                if (scrollPos.y > 0.995f)
                    OnRequireMoreMessages?.Invoke();
            });

            collapsableAreaOriginalWidth = collapsableArea.sizeDelta.x;
            membersIconButton.onClick.AddListener(ToggleMembersSection);
            expandMembersListButton.onClick.AddListener(ToggleMembersSection);
            collapseMembersListButton.onClick.AddListener(ToggleMembersSection);
        }

        public override void RefreshControl()
        {
            nameLabel.text = $"#{model.name}";
            memberCountLabel.text = model.memberCount.ToString();
        }

        public void Hide() => gameObject.SetActive(false);

        public void Show() => gameObject.SetActive(true);
        
        public void Setup(PublicChatModel model)
        {
            this.model = model;
            RefreshControl();
        }

        public void ActivatePreview()
        {
            isPreviewActivated = true;
            const float alphaTarget = 0f;

            if (!gameObject.activeInHierarchy)
            {
                foreach (var group in previewCanvasGroup)
                    group.alpha = alphaTarget;

                return;
            }

            if (alphaRoutine != null)
                StopCoroutine(alphaRoutine);

            alphaRoutine = StartCoroutine(SetAlpha(alphaTarget, 0.5f));
            ((RectTransform) transform).sizeDelta = previewModeSize;
        }

        public void DeactivatePreview()
        {
            isPreviewActivated = false;
            const float alphaTarget = 1f;

            if (!gameObject.activeInHierarchy)
            {
                foreach (var group in previewCanvasGroup)
                    group.alpha = alphaTarget;

                return;
            }

            if (alphaRoutine != null)
                StopCoroutine(alphaRoutine);

            alphaRoutine = StartCoroutine(SetAlpha(alphaTarget, 0.5f));
            ((RectTransform) transform).sizeDelta = originalSize;
        }

        public void SetLoadingMessagesActive(bool isActive)
        {
            if (messagesLoading == null) return;
            messagesLoading.SetActive(isActive);
        }

        public void SetOldMessagesLoadingActive(bool isActive)
        {
            if (oldMessagesLoadingContainer == null) return;
            oldMessagesLoadingContainer.SetActive(isActive);
            oldMessagesLoadingContainer.transform.SetAsFirstSibling();
        }

        public void Configure(PublicChatModel newModel) => Setup(newModel);

        public void OnPointerDown(PointerEventData eventData) => OnFocused?.Invoke(true);

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            OnFocused?.Invoke(false);
        }
        
        private IEnumerator SetAlpha(float target, float duration)
        {
            var t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;

                foreach (var group in previewCanvasGroup)
                    group.alpha = Mathf.Lerp(group.alpha, target, t / duration);

                yield return null;
            }

            foreach (var group in previewCanvasGroup)
                group.alpha = target;
        }

        private void ShowOptionsMenu()
        {
            contextualMenu.SetHeaderTitle($"#{model.channelId}");
            contextualMenu.Show();
        }

        private void ToggleMembersSection()
        {
            isMembersSectionOpen = !isMembersSectionOpen;

            expandMembersListButton.gameObject.SetActive(!isMembersSectionOpen);
            collapseMembersListButton.gameObject.SetActive(isMembersSectionOpen);

            collapsableArea.sizeDelta = new Vector2(
                isMembersSectionOpen ? 
                    collapsableAreaOriginalWidth + MEMBERS_SECTION_WIDTH :
                    collapsableAreaOriginalWidth,
                collapsableArea.sizeDelta.y);

            if (isMembersSectionOpen)
                OnShowMembersList?.Invoke();
            else
                OnHideMembersList?.Invoke();
        }
    }
}