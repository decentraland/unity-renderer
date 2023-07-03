using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestEntry : FriendEntryBase
{
    [SerializeField] internal TMP_Text bodyMessage;
    [SerializeField] internal TMP_Text requestDate;
    [SerializeField] internal GameObject shortcutButtonsContainer;
    [SerializeField] internal Button acceptButton;
    [SerializeField] internal Button rejectButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button openButton;

    public bool isReceived { get; private set; }

    public event Action<FriendRequestEntry> OnAccepted;
    public event Action<FriendRequestEntry> OnRejected;
    public event Action<FriendRequestEntry> OnCancelled;
    public event Action<FriendRequestEntry> OnOpened;

    public override void Awake()
    {
        base.Awake();

        acceptButton.onClick.RemoveAllListeners();
        rejectButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() => OnAccepted?.Invoke(this));
        rejectButton.onClick.AddListener(() => OnRejected?.Invoke(this));
        cancelButton.onClick.AddListener(() => OnCancelled?.Invoke(this));
        openButton.onClick.AddListener(() => OnOpened?.Invoke(this));
    }

    public void Populate(FriendRequestEntryModel model)
    {
        base.Populate(model);
        SetBodyMessage(model.bodyMessage);
        SetRequestDate(model.timestamp);
        SetReceived(model.isReceived);
        SetShortcutButtonsActive(model.isShortcutButtonsActive);
    }

    public override void Populate(FriendEntryModel model)
    {
        base.Populate(model);

        if (model is FriendRequestEntryModel requestModel)
        {
            SetBodyMessage(requestModel.bodyMessage);
            SetRequestDate(requestModel.timestamp);
            SetReceived(requestModel.isReceived);
            SetShortcutButtonsActive(requestModel.isShortcutButtonsActive);
        }
    }

    private void SetBodyMessage(string value)
    {
        if (bodyMessage == null)
            return;

        bodyMessage.text = value;
        bodyMessage.gameObject.SetActive(!string.IsNullOrEmpty(value));
    }

    private void SetRequestDate(DateTime value)
    {
        if (requestDate == null)
            return;

        DateTime localTime = value.ToLocalTime();
        requestDate.text = localTime.ToString("MMM dd", new CultureInfo("en-US")).ToUpper();
    }

    private void SetReceived(bool value)
    {
        isReceived = value;

        if (isReceived)
            PopulateReceived();
        else
            PopulateSent();
    }

    private void PopulateReceived()
    {
        isReceived = true;
        cancelButton.gameObject.SetActive(false);
        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);
    }

    private void PopulateSent()
    {
        isReceived = false;
        cancelButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
    }

    private void SetShortcutButtonsActive(bool isActive)
    {
        if (shortcutButtonsContainer != null)
            shortcutButtonsContainer.SetActive(isActive);

        if (requestDate != null)
            requestDate.gameObject.SetActive(!isActive);
    }
}
