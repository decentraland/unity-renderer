using DCL.Social.Chat;
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
        [SerializeField] internal ToggleComponentView muteToggle;

        private Coroutine alphaRoutine;
        private bool isMembersSectionOpen;
        private float collapsableAreaOriginalWidth;

        public event Action OnClose;
        public event Action<bool> OnFocused;
        public event Action OnBack;
        public event Action OnRequireMoreMessages;
        public event Action OnLeaveChannel;
        public event Action OnShowMembersList;
        public event Action OnHideMembersList;
        public event Action<bool> OnMuteChanged;

        public bool IsActive => gameObject.activeInHierarchy;
        public IChatHUDComponentView ChatHUD => chatView;
        public IChannelMembersComponentView ChannelMembersHUD => membersList;
        public RectTransform Transform => (RectTransform) transform;
        public bool IsFocused => isFocused;
        private Color targetGraphicColor;

        public override void Awake()
        {
            base.Awake();
            backButton.onClick.AddListener(() => OnBack?.Invoke());
            closeButton.onClick.AddListener(() => OnClose?.Invoke());
            contextualMenu.OnLeave += () => OnLeaveChannel?.Invoke();
            optionsButton.onClick.AddListener(ShowOptionsMenu);
            // TODO: It was decided to temporally remove the loading of the channel's history. We'll re-enable it later.
            //scroll.onValueChanged.AddListener(scrollPos =>
            //{
            //    if (scrollPos.y > 0.995f)
            //        OnRequireMoreMessages?.Invoke();
            //});

            collapsableAreaOriginalWidth = collapsableArea.sizeDelta.x;
            membersIconButton.onClick.AddListener(ToggleMembersSection);
            expandMembersListButton.onClick.AddListener(ToggleMembersSection);
            collapseMembersListButton.onClick.AddListener(ToggleMembersSection);
            muteToggle.OnSelectedChanged += (b, s, arg3) => OnMuteChanged?.Invoke(b);
            targetGraphicColor = membersIconButton.targetGraphic.color;
        }

        public override void RefreshControl()
        {
            nameLabel.text = $"#{model.name}";
            memberCountLabel.text = model.memberCount.ToString();
            muteToggle.SetIsOnWithoutNotify(model.muted);
        }

        public void Hide() => gameObject.SetActive(false);

        public void Show()
        {
            gameObject.SetActive(true);

            if (!isMembersSectionOpen)
                ToggleMembersSection();
        }

        public void Setup(PublicChatModel model)
        {
            this.model = model;
            RefreshControl();
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

        private void ShowOptionsMenu()
        {
            contextualMenu.SetHeaderTitle($"#{model.name}");
            contextualMenu.Show();
        }

        private void ToggleMembersSection()
        {
            isMembersSectionOpen = !isMembersSectionOpen;

            expandMembersListButton.gameObject.SetActive(!isMembersSectionOpen);
            collapseMembersListButton.gameObject.SetActive(isMembersSectionOpen);

            contextualMenu.gameObject.transform.SetParent(collapsableArea);

            collapsableArea.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                isMembersSectionOpen ? collapsableAreaOriginalWidth + MEMBERS_SECTION_WIDTH : collapsableAreaOriginalWidth);

            contextualMenu.gameObject.transform.SetParent(this.transform);

            if (isMembersSectionOpen)
            {
                targetGraphicColor.a = 1f;
                membersIconButton.targetGraphic.color = targetGraphicColor;
                OnShowMembersList?.Invoke();
            }
            else
            {
                targetGraphicColor.a = 0f;
                membersIconButton.targetGraphic.color = targetGraphicColor;
                OnHideMembersList?.Invoke();
            }
        }
    }
}
