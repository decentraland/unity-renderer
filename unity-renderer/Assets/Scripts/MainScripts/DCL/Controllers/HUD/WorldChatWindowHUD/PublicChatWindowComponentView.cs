using DCL.Chat.HUD;
using DCL.Social.Chat;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PublicChatWindowComponentView : BaseComponentView, IPublicChatWindowView, IComponentModelConfig<PublicChatModel>, IPointerDownHandler
{
    private const int MEMBERS_SECTION_WIDTH = 280;

    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button backButton;
    [SerializeField] internal TMP_Text nameLabel;
    [SerializeField] internal ChatHUDView chatView;
    [SerializeField] internal PublicChatModel model;
    [SerializeField] internal ToggleComponentView muteToggle;
    [SerializeField] internal RectTransform collapsableArea;
    [SerializeField] internal ButtonComponentView expandMembersListButton;
    [SerializeField] internal ButtonComponentView collapseMembersListButton;
    [SerializeField] internal ChannelMembersComponentView membersList;
    [SerializeField] internal ButtonComponentView goToCrowdButton;
    [SerializeField] internal TMP_Text memberCountLabel;
    [SerializeField] internal Button membersIconButton;

    private Coroutine alphaRoutine;

    public event Action OnClose;
    public event Action OnBack;
    public event Action<bool> OnFocused
    {
        add => onFocused += value;
        remove => onFocused -= value;
    }
    public event Action OnClickOverWindow;
    public event Action<bool> OnMuteChanged;
    public event Action OnShowMembersList;
    public event Action OnHideMembersList;
    public event Action OnGoToCrowd;

    public bool IsActive => gameObject.activeInHierarchy;
    public IChatHUDComponentView ChatHUD => chatView;
    public RectTransform Transform => (RectTransform) transform;
    public IChannelMembersComponentView ChannelMembersHUD => membersList;

    private bool isMembersSectionOpen;
    private float collapsableAreaOriginalWidth;
    private Color targetGraphicColor;

    public override void Awake()
    {
        base.Awake();

        backButton.onClick.AddListener(() => OnBack?.Invoke());
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        muteToggle.OnSelectedChanged += (b, s, arg3) => OnMuteChanged?.Invoke(b);
        expandMembersListButton.onClick.AddListener(ToggleMembersSection);
        collapseMembersListButton.onClick.AddListener(ToggleMembersSection);
        collapsableAreaOriginalWidth = collapsableArea.sizeDelta.x;
        membersIconButton.onClick.AddListener(ToggleMembersSection);
        targetGraphicColor = membersIconButton.targetGraphic.color;

        if (goToCrowdButton != null)
            goToCrowdButton.onClick.AddListener(() => OnGoToCrowd?.Invoke());
    }

    public override void RefreshControl()
    {
        nameLabel.text = $"~{model.name}";
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

    public void Configure(PublicChatModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public void OnPointerDown(PointerEventData eventData) => OnClickOverWindow?.Invoke();

    public void UpdateMembersCount(int membersAmount) =>
        memberCountLabel.text = membersAmount.ToString();

    private void ToggleMembersSection()
    {
        isMembersSectionOpen = !isMembersSectionOpen;

        expandMembersListButton.gameObject.SetActive(!isMembersSectionOpen);
        collapseMembersListButton.gameObject.SetActive(isMembersSectionOpen);

        collapsableArea.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            isMembersSectionOpen ? collapsableAreaOriginalWidth + MEMBERS_SECTION_WIDTH : collapsableAreaOriginalWidth);

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
