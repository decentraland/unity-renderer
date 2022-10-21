using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PublicChatWindowComponentView : BaseComponentView, IPublicChatWindowView, IComponentModelConfig<PublicChatModel>, IPointerDownHandler
{
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button backButton;
    [SerializeField] internal TMP_Text nameLabel;
    [SerializeField] internal ChatHUDView chatView;
    [SerializeField] internal PublicChatModel model;
    [SerializeField] internal ToggleComponentView muteToggle;
    
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

    public bool IsActive => gameObject.activeInHierarchy;
    public IChatHUDComponentView ChatHUD => chatView;
    public RectTransform Transform => (RectTransform) transform;

    public static PublicChatWindowComponentView Create()
    {
        return Instantiate(Resources.Load<PublicChatWindowComponentView>("SocialBarV1/GeneralChatChannelHUD"));
    }

    public override void Awake()
    {
        base.Awake();
        backButton.onClick.AddListener(() => OnBack?.Invoke());
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        muteToggle.OnSelectedChanged += (b, s, arg3) => OnMuteChanged?.Invoke(b);
    }
    
    public override void RefreshControl()
    {
        nameLabel.text = $"~{model.name}";
        muteToggle.SetIsOnWithoutNotify(model.muted);
    }

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);
    
    public void Configure(PublicChatModel model)
    {
        this.model = model;
        RefreshControl();
    }
    
    public void OnPointerDown(PointerEventData eventData) => OnClickOverWindow?.Invoke();
  
}