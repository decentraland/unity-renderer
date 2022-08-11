using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PublicChatChannelComponentView : BaseComponentView, IChannelChatWindowView, IComponentModelConfig<PublicChatChannelModel>, IPointerDownHandler
{
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button backButton;
    [SerializeField] internal TMP_Text nameLabel;
    [SerializeField] internal TMP_Text descriptionLabel;
    [SerializeField] internal ChatHUDView chatView;
    [SerializeField] internal PublicChatChannelModel model;
    [SerializeField] internal CanvasGroup[] previewCanvasGroup;
    [SerializeField] internal Vector2 previewModeSize;
    
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

    public static PublicChatChannelComponentView Create()
    {
        return Instantiate(Resources.Load<PublicChatChannelComponentView>("SocialBarV1/GeneralChatChannelHUD"));
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
        nameLabel.text = $"#{model.name}";
        descriptionLabel.text = model.description;
    }

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);
    
    public void Configure(PublicChatChannelModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public void ActivatePreview()
    {
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

    public void ActivatePreviewInstantly()
    {
        if (alphaRoutine != null)
            StopCoroutine(alphaRoutine);
        
        const float alphaTarget = 0f;
        foreach (var group in previewCanvasGroup)
            group.alpha = alphaTarget;

        ((RectTransform) transform).sizeDelta = previewModeSize;
    }

    public void DeactivatePreview()
    {
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
    
    public void OnPointerDown(PointerEventData eventData) => OnClickOverWindow?.Invoke();
    
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
}