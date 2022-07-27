using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChatNotificationMessageComponentView : BaseComponentView, IChatNotificationMessageComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal Button button;
    [SerializeField] internal TMP_Text notificationMessage;
    [SerializeField] internal TMP_Text notificationHeader;
    [SerializeField] internal TMP_Text notificationTimestamp;
    [SerializeField] internal ImageComponentView image;
    [SerializeField] internal bool isPrivate;

    [Header("Configuration")]
    [SerializeField] internal ChatNotificationMessageComponentModel model;

    public event Action<string> OnClickedNotification;
    public string notificationTargetId;
    private int maxContentCharacters, maxHeaderCharacters;

    public void Configure(BaseComponentModel newModel)
    {
        model = (ChatNotificationMessageComponentModel)newModel;
        RefreshControl();
    }

    public override void Awake()
    {
        base.Awake();

        button?.onClick.AddListener(()=>OnClickedNotification.Invoke(notificationTargetId));
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
    }

    public void SetTimestamp(string timestamp)
    {
        model.time = timestamp;
        notificationTimestamp.text = timestamp;
    }

    public void SetNotificationHeader(string header)
    {
        model.messageHeader = header;
        if(header.Length <= maxHeaderCharacters)
            notificationHeader.text = header;
        else
            notificationHeader.text = $"{header.Substring(0, maxHeaderCharacters)}...";
    }

    public void SetIsPrivate(bool isPrivate)
    {
        model.isPrivate = isPrivate;
        this.isPrivate = isPrivate;
        image.gameObject.SetActive(isPrivate);
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

}
