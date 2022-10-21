using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DCL;
using DCL.Interface;

public class PublicChatWindowComponentView : BaseComponentView, IPublicChatWindowView, IComponentModelConfig<PublicChatModel>, IPointerDownHandler
{
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button backButton;
    [SerializeField] internal TMP_Text nameLabel;
    [SerializeField] internal ChatHUDView chatView;
    [SerializeField] internal PublicChatModel model;
    
    private Coroutine alphaRoutine;
    private Vector2 originalSize;
    internal BaseVariable<string> openedChat => DataStore.i.HUDs.openedChat;

    public event Action OnClose;
    public event Action OnBack;
    public event Action<bool> OnFocused
    {
        add => onFocused += value;
        remove => onFocused -= value;
    }
    public event Action OnClickOverWindow;

    public bool IsActive => gameObject.activeInHierarchy;
    public IChatHUDComponentView ChatHUD => chatView;
    public RectTransform Transform => (RectTransform) transform;
    public bool IsFocused => isFocused;

    public static PublicChatWindowComponentView Create()
    {
        return Instantiate(Resources.Load<PublicChatWindowComponentView>("SocialBarV1/GeneralChatChannelHUD"));
    }

    public override void Awake()
    {
        base.Awake();
        originalSize = ((RectTransform) transform).sizeDelta;
        backButton.onClick.AddListener(() => OnBack?.Invoke());
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
    }
    
    public override void RefreshControl()
    {
        nameLabel.text = $"~{model.name}";
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