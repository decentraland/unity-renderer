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

public class ChatNotificationMessageComponentView : BaseComponentView, IChatNotificationMessageComponentView, IComponentModelConfig<ChatNotificationMessageComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] internal Button button;
    [SerializeField] public TMP_Text notificationMessage;
    [SerializeField] internal TMP_Text notificationHeader;
    [SerializeField] internal TMP_Text notificationSender;
    [SerializeField] internal TMP_Text notificationTimestamp;
    [SerializeField] internal ImageComponentView image;
    [SerializeField] internal GameObject imageBackground;
    [SerializeField] internal GameObject firstSeparator;
    [SerializeField] internal GameObject secondSeparator;
    [SerializeField] internal bool isPrivate;
    [SerializeField] internal RectTransform backgroundTransform;
    [SerializeField] internal RectTransform messageContainerTransform;
    [SerializeField] internal RectTransform header;
    [SerializeField] internal RectTransform content;

    [Header("Configuration")]
    [SerializeField] internal ChatNotificationMessageComponentModel model;
    [SerializeField] private Color privateColor;
    [SerializeField] private Color publicColor;

    public event Action<string> OnClickedNotification;
    public string notificationTargetId;
    private int maxContentCharacters, maxHeaderCharacters, maxSenderCharacters;
    private float startingXPosition;

    public void Configure(ChatNotificationMessageComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void Awake()
    {
        base.Awake();
        button?.onClick.AddListener(()=>OnClickedNotification?.Invoke(notificationTargetId));
        startingXPosition = messageContainerTransform.anchoredPosition.x;
        RefreshControl();
    }

    public override void Show(bool instant = false)
    {
        showHideAnimator.animSpeedFactor = 0.7f;
        base.Show(instant);
        ForceUIRefresh();
    }

    public override void OnFocus()
    {
        base.OnFocus();
        messageContainerTransform.anchoredPosition = new Vector2(startingXPosition + 5, messageContainerTransform.anchoredPosition.y);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        messageContainerTransform.anchoredPosition = new Vector2(startingXPosition, messageContainerTransform.anchoredPosition.y);
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
        SetMaxSenderCharacters(model.maxSenderCharacters);
        SetNotificationSender(model.messageSender);
        SetMessage(model.message);
        SetTimestamp(model.time);
        SetIsPrivate(model.isPrivate);
        SetNotificationHeader(model.messageHeader);
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

    public void SetNotificationSender(string sender)
    {
        model.messageSender = sender;
        if (sender.Length <= maxSenderCharacters)
            notificationSender.text = $"{sender}:";
        else
            notificationSender.text = $"{sender.Substring(0, maxSenderCharacters)}:";

        ForceUIRefresh();
    }

    public void SetIsPrivate(bool isPrivate)
    {
        model.isPrivate = isPrivate;
        this.isPrivate = isPrivate;
        imageBackground.SetActive(isPrivate);
        firstSeparator.SetActive(isPrivate);
        secondSeparator.SetActive(isPrivate);
        if (isPrivate)
            notificationHeader.color = privateColor;
        else
            notificationHeader.color = publicColor;
        ForceUIRefresh();
    }

    public void SetImage(string uri)
    {
        if (!isPrivate)
            return;

        model.imageUri = uri;
        image.SetImage(uri);
    }

    public void SetPositionOffset(float xPosHeader, float xPosContent)
    {
        if(header != null)
            header.anchoredPosition = new Vector2(xPosHeader, header.anchoredPosition.y);
        if(content != null)
            content.anchoredPosition = new Vector2(xPosContent, content.anchoredPosition.y);

        ForceUIRefresh();
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

    public void SetMaxSenderCharacters(int maxSenderCharacters)
    {
        model.maxSenderCharacters = maxSenderCharacters;
        this.maxSenderCharacters = maxSenderCharacters;
    }

    public void SetNotificationTargetId(string notificationTargetId)
    {
        model.notificationTargetId = notificationTargetId;
        this.notificationTargetId = notificationTargetId;
    }

    public void ForceUIRefresh()
    {
        Utils.ForceRebuildLayoutImmediate(backgroundTransform);
    }
}
