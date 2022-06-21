using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IChatNotificationMessageComponentView
{
    /// <summary>
    /// Event that will be triggered when the notification is clicked.
    /// </summary>
    Button.ButtonClickedEvent onClick { get; }

    /// <summary>
    /// Set the notification text.
    /// </summary>
    /// <param name="message">New message.</param>
    void SetMessage(string message);

    /// <summary>
    /// Set the notification time.
    /// </summary>
    /// <param name="timestamp">New timestamp.</param>
    void SetTimestamp(string timestamp);

    /// <summary>
    /// Set the notification header, can be a channel name or a user name.
    /// </summary>
    /// <param name="header">New header.</param>
    void SetNotificationHeader(string header);

    /// <summary>
    /// Set the notification type, can be a private or not.
    /// </summary>
    /// <param name="isPrivate">If the notification is private or not.</param>
    void SetIsPrivate(bool isPrivate);

    /// <summary>
    /// Set the notification player icon if isPrivate is true.
    /// </summary>
    /// <param name="newIcon">New Icon. Null for hide the icon.</param>
    void SetImage(Sprite newIcon);

    /// <summary>
    /// Set the notification maximum content characters.
    /// </summary>
    /// <param name="maxContentCharacters">Max content characters</param>
    void SetMaxContentCharacters(int maxContentCharacters);

    /// <summary>
    /// Set the notification maximum header characters.
    /// </summary>
    /// <param name="maxHeaderCharacters">Max header characters</param>
    void SetMaxHeaderCharacters(int maxHeaderCharacters);

    /// <summary>
    /// Set the notification target id (either a channel or a user id)
    /// </summary>
    /// <param name="notificationTargetId">New target id.</param>
    void SetNotificationTargetId(string notificationTargetId);
}

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

    public Button.ButtonClickedEvent onClick => button?.onClick;
    public string notificationTargetId;
    private int maxContentCharacters, maxHeaderCharacters;

    public void Configure(BaseComponentModel newModel)
    {
        model = (ChatNotificationMessageComponentModel)newModel;
        RefreshControl();
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
        SetImage(model.profileIcon);
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

    public void SetImage(Sprite newImage)
    {
        if (!isPrivate)
            return;

        model.profileIcon = newImage;
        image.SetImage(newImage);
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
