using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DG.Tweening;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ChatNotificationMessageComponentView : BaseComponentView, IChatNotificationMessageComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal Button button;
    [SerializeField] public TMP_Text notificationMessage;
    [SerializeField] internal TMP_Text notificationHeader;
    [SerializeField] internal TMP_Text notificationTimestamp;
    [SerializeField] internal ImageComponentView image;
    [SerializeField] internal GameObject imageBackground;
    [SerializeField] internal bool isPrivate;
    [SerializeField] internal RectTransform backgroundTransform;
    [SerializeField] internal GridLayoutGroup layoutGroup;

    [Header("Configuration")]
    [SerializeField] internal ChatNotificationMessageComponentModel model;

    public event Action<string> OnClickedNotification;
    public string notificationTargetId;
    private int maxContentCharacters, maxHeaderCharacters;
    private float startingXPosition;
    private CancellationTokenSource ct = new CancellationTokenSource();

    public void Configure(BaseComponentModel newModel)
    {
        model = (ChatNotificationMessageComponentModel)newModel;
        RefreshControl();
    }

    public override void Awake()
    {
        base.Awake();
        button?.onClick.AddListener(()=>OnClickedNotification.Invoke(notificationTargetId));
        startingXPosition = backgroundTransform.position.x;
        RefreshControl();
    }

    public override void Show(bool instant = false)
    {
        showHideAnimator.animSpeedFactor = 0.7f;
        base.Show(instant);
    }

    public override void OnFocus()
    {
        base.OnFocus();
        ct.Cancel();
        ct = new CancellationTokenSource();
        AnimateSelect(20, ct.Token).Forget();
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        //transform.DOMoveX(startingXPosition, 1);
    }

    private async UniTaskVoid AnimateSelect(int targetValue, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            int currentValue = layoutGroup.padding.left;
            DOTween.To(() => currentValue, x => currentValue = x, targetValue, 0.8f).SetEase(Ease.OutCubic);
            while (layoutGroup.padding.left != targetValue)
            {
                layoutGroup.padding.left = currentValue;
                ForceUIRefresh();
                await UniTask.NextFrame(cancellationToken);
            }
        }
        catch (OperationCanceledException ex)
        {
        }
    }

    public override void Hide(bool instant = false)
    {
        showHideAnimator.animSpeedFactor = 0.05f;
        base.Hide(instant);
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetMaxContentCharacters(model.maxContentCharacters);
        SetMaxHeaderCharacters(model.maxHeaderCharacters);
        SetMessage(model.message);
        SetTimestamp(model.time);
        SetNotificationHeader(model.messageHeader);
        SetIsPrivate(model.isPrivate);
        SetImage(model.imageUri);
    }

    public void SetMessage(string message) 
    {
        model.message = message;
        if (message.Length <= maxContentCharacters)
            notificationMessage.text = message;
        else
            notificationMessage.text = $"{message.Substring(0, maxContentCharacters)}...";

        ForceUIRefresh();
    }

    public void SetTimestamp(string timestamp)
    {
        model.time = timestamp;
        notificationTimestamp.text = timestamp;

        ForceUIRefresh();
    }

    public void SetNotificationHeader(string header)
    {
        model.messageHeader = header;
        if(header.Length <= maxHeaderCharacters)
            notificationHeader.text = header;
        else
            notificationHeader.text = $"{header.Substring(0, maxHeaderCharacters)}...";

        ForceUIRefresh();
    }

    public void SetIsPrivate(bool isPrivate)
    {
        model.isPrivate = isPrivate;
        this.isPrivate = isPrivate;
        imageBackground.SetActive(isPrivate);

        ForceUIRefresh();
    }

    public void SetImage(string uri)
    {
        if (!isPrivate)
            return;

        model.imageUri = uri;
        image.SetImage(uri);
    }

    public void SetMaxContentCharacters(int maxContentCharacters)
    {
        model.maxContentCharacters = maxContentCharacters;
        this.maxContentCharacters = maxContentCharacters;
    }

    public void SetMaxHeaderCharacters(int maxHeaderCharacters)
    {
        model.maxHeaderCharacters = maxHeaderCharacters;
        this.maxHeaderCharacters = maxHeaderCharacters;
    }

    public void SetNotificationTargetId(string notificationTargetId)
    {
        model.notificationTargetId = notificationTargetId;
        this.notificationTargetId = notificationTargetId;
    }

    private void ForceUIRefresh()
    {
        Utils.ForceRebuildLayoutImmediate(backgroundTransform);
    }
}
