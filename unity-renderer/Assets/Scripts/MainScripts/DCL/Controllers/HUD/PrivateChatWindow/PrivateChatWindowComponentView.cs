using System;
using System.Collections;
using SocialBar.UserThumbnail;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PrivateChatWindowComponentView : BaseComponentView, IPrivateChatComponentView, IPointerDownHandler
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private UserThumbnailComponentView userThumbnail;
    [SerializeField] private TMP_Text userNameLabel;
    [SerializeField] private PrivateChatHUDView chatView;
    [SerializeField] private GameObject jumpInButtonContainer;
    [SerializeField] private UserContextMenu userContextMenu;
    [SerializeField] private RectTransform userContextMenuReferencePoint;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Model model;
    [SerializeField] private CanvasGroup[] previewCanvasGroup;
    [SerializeField] private Vector2 previewModeSize;
    
    private Coroutine alphaRoutine;
    private Vector2 originalSize;

    public event Action OnPressBack;
    public event Action OnMinimize;
    public event Action OnClose;
    public event Action<string> OnUnfriend
    {
        add => userContextMenu.OnUnfriend += value;
        remove => userContextMenu.OnUnfriend -= value;
    }

    public event Action<bool> OnFocused;

    public IChatHUDComponentView ChatHUD => chatView;
    public bool IsActive => gameObject.activeInHierarchy;
    public RectTransform Transform => (RectTransform) transform;
    public bool IsFocused => isFocused;

    public static PrivateChatWindowComponentView Create()
    {
        return Instantiate(Resources.Load<PrivateChatWindowComponentView>("SocialBarV1/PrivateChatHUD"));
    }

    public override void Awake()
    {
        base.Awake();
        originalSize = ((RectTransform) transform).sizeDelta;
        backButton.onClick.AddListener(() => OnPressBack?.Invoke());
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        optionsButton.onClick.AddListener(ShowOptions);
        userContextMenu.OnBlock += HandleBlockFromContextMenu;
    }

    public override void Dispose()
    {
        if (!this) return;
        if (!gameObject) return;
        
        if (userContextMenu != null)
        {
            userContextMenu.OnBlock -= HandleBlockFromContextMenu;
        }
            
        base.Dispose();
    }

    public void ActivatePreview()
    {
        if (!this) return;
        if (!gameObject) return;
        
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

    public override void RefreshControl()
    {
        userThumbnail.Configure(new UserThumbnailComponentModel
        {
            faceUrl = model.faceSnapshotUrl,
            isBlocked = model.isUserBlocked,
            isOnline = model.isUserOnline
        });
        userNameLabel.SetText(model.userName);
        jumpInButtonContainer.SetActive(model.isUserOnline);
    }

    public void Setup(UserProfile profile, bool isOnline, bool isBlocked)
    {
        model = new Model
        {
            userId = profile.userId,
            faceSnapshotUrl = profile.face256SnapshotURL,
            userName = profile.userName,
            isUserOnline = isOnline,
            isUserBlocked = isBlocked
        };
        RefreshControl();
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnFocused?.Invoke(false);
    }

    public void OnPointerDown(PointerEventData eventData) => OnFocused?.Invoke(true);

    private void ShowOptions()
    {
        var contextMenuTransform = (RectTransform) userContextMenu.transform;
        contextMenuTransform.pivot = userContextMenuReferencePoint.pivot;
        contextMenuTransform.position = userContextMenuReferencePoint.position;
        userContextMenu.Show(model.userId);
    }

    private void HandleBlockFromContextMenu(string userId, bool isBlocked)
    {
        if (userId != model.userId) return;
        model.isUserBlocked = isBlocked;
        RefreshControl();
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

    [Serializable]
    private struct Model
    {
        public string userId;
        public string userName;
        public string faceSnapshotUrl;
        public bool isUserBlocked;
        public bool isUserOnline;
    }
}