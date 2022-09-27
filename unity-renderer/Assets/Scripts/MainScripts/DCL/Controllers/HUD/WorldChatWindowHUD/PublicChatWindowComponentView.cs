using System;
using System.Collections;
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
    
    private Coroutine alphaRoutine;
    private Vector2 originalSize;

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

    public void ActivatePreview()
    {
        Debug.Log("aaa");
        Hide();
    }

    public void ActivatePreviewInstantly()
    {
        Debug.Log("bbb");
        Hide();
    }

    public void DeactivatePreview()
    {
        Debug.Log("ccc");
        const float alphaTarget = 1f;
        
        if (!gameObject.activeInHierarchy)
        {
            
            return;
        }
        
        if (alphaRoutine != null)
            StopCoroutine(alphaRoutine);
        
        alphaRoutine = StartCoroutine(SetAlpha(alphaTarget, 0.5f));
        ((RectTransform) transform).sizeDelta = originalSize;
    }
    
    public void OnPointerDown(PointerEventData eventData) => OnClickOverWindow?.Invoke();
  
    private IEnumerator SetAlpha(float target, float duration)
    {
        var t = 0f;
        
        while (t < duration)
        {
            t += Time.deltaTime;
            
            yield return null;
        }
    }
}