using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PublicChatChannelComponentView : BaseComponentView, IChannelChatWindowView, IComponentModelConfig
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private ChatHUDView chatView;
    [SerializeField] private PublicChatChannelModel model;
    [SerializeField] private CanvasGroup[] previewCanvasGroup;
    
    private Coroutine alphaRoutine;

    public event Action OnClose;
    public event Action OnBack;
    public event Action<bool> OnFocused
    {
        add => onFocused += value;
        remove => onFocused -= value;
    }

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
    }

    public void Configure(BaseComponentModel newModel) => Configure((PublicChatChannelModel) newModel);
    
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