using DCL.Helpers;
using DCL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ChatEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public struct Model
    {
        public enum SubType
        {
            NONE,
            PRIVATE_FROM,
            PRIVATE_TO
        }

        public ChatMessage.Type messageType;
        public string bodyText;
        public string senderName;
        public string recipientName;
        public string otherUserId;
        public ulong timestamp;

        public SubType subType;
    }

    [SerializeField] internal float timeToFade = 10f;
    [SerializeField] internal float fadeDuration = 5f;

    [SerializeField] internal TextMeshProUGUI username;
    [SerializeField] internal TextMeshProUGUI body;

    [SerializeField] internal Color worldMessageColor = Color.white;

    [FormerlySerializedAs("privateMessageColor")]
    [SerializeField] internal Color privateToMessageColor = Color.white;

    [FormerlySerializedAs("privateMessageColor")]
    [SerializeField] internal Color privateFromMessageColor = Color.white;

    [SerializeField] internal Color systemColor = Color.white;
    [SerializeField] internal Color playerNameColor = Color.yellow;
    [SerializeField] internal Color nonPlayerNameColor = Color.white;
    [SerializeField] CanvasGroup group;
    [SerializeField] internal float timeToHoverPanel = 1f;

    [NonSerialized] public string messageLocalDateTime;

    bool fadeEnabled = false;
    double fadeoutStartTime;
    float hoverPanelTimer = 0;

    public RectTransform hoverPanelPositionReference;
    public Model model { get; private set; }

    public event UnityAction<string> OnPress;
    public event UnityAction<ChatEntry> OnTriggerHover;
    public event UnityAction OnCancelHover;

    public void Populate(Model chatEntryModel)
    {
        this.model = chatEntryModel;

        string userString = GetDefaultSenderString(chatEntryModel.senderName);

        if (chatEntryModel.subType == Model.SubType.PRIVATE_FROM)
        {
            userString = $"<b>From {chatEntryModel.senderName}:</b>";
        }
        else
        if (chatEntryModel.subType == Model.SubType.PRIVATE_TO)
        {
            userString = $"<b>To {chatEntryModel.recipientName}:</b>";
        }

        switch (chatEntryModel.messageType)
        {
            case ChatMessage.Type.PUBLIC:
                body.color = worldMessageColor;

                if (username != null)
                    username.color = chatEntryModel.senderName == UserProfile.GetOwnUserProfile().userName ? playerNameColor : nonPlayerNameColor;
                break;
            case ChatMessage.Type.PRIVATE:
                body.color = worldMessageColor;

                if (username != null)
                {
                    if (model.subType == Model.SubType.PRIVATE_TO)
                        username.color = privateToMessageColor;
                    else
                        username.color = privateFromMessageColor;
                }

                break;
            case ChatMessage.Type.SYSTEM:
                body.color = systemColor;

                if (username != null)
                    username.color = systemColor;
                break;
        }

        chatEntryModel.bodyText = RemoveTabs(chatEntryModel.bodyText);

        if (username != null && !string.IsNullOrEmpty(userString))
        {
            if (username != null)
                username.text = userString;

            body.text = $"{userString} {chatEntryModel.bodyText}";
        }
        else
        {
            if (username != null)
                username.text = "";

            body.text = $"{chatEntryModel.bodyText}";
        }

        messageLocalDateTime = UnixTimeStampToLocalDateTime(chatEntryModel.timestamp).ToString();

        Utils.ForceUpdateLayout(transform as RectTransform);

        if (fadeEnabled)
            group.alpha = 0;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (model.messageType != ChatMessage.Type.PRIVATE) return;

        OnPress?.Invoke(model.otherUserId);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        hoverPanelTimer = timeToHoverPanel;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        hoverPanelTimer = 0f;

        OnCancelHover?.Invoke();
    }

    void OnDisable()
    {
        OnPointerExit(null);
    }

    public void SetFadeout(bool enabled)
    {
        if (!enabled)
        {
            group.alpha = 1;
            group.blocksRaycasts = true;
            group.interactable = true;
            fadeEnabled = false;
            return;
        }

        fadeoutStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        fadeEnabled = true;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    void Update()
    {
        Fade();

        ProcessHoverPanelTimer();
    }

    void Fade()
    {
        if (!fadeEnabled) return;

        //NOTE(Brian): Small offset using normalized Y so we keep the cascade effect
        double yOffset = (transform as RectTransform).anchoredPosition.y / (double)Screen.height * 2.0;

        double fadeTime = Math.Max(model.timestamp / 1000.0, fadeoutStartTime) + timeToFade - yOffset;
        double currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        if (currentTime > fadeTime)
        {
            double timeSinceFadeTime = currentTime - fadeTime;
            group.alpha = Mathf.Clamp01(1 - (float)(timeSinceFadeTime / fadeDuration));
        }
        else
        {
            group.alpha += (1 - group.alpha) * 0.05f;
        }
    }

    void ProcessHoverPanelTimer()
    {
        if (hoverPanelTimer <= 0f) return;

        hoverPanelTimer -= Time.deltaTime;
        if (hoverPanelTimer <= 0f)
        {
            hoverPanelTimer = 0f;

            OnTriggerHover?.Invoke(this);
        }
    }

    string RemoveTabs(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        //NOTE(Brian): ContentSizeFitter doesn't fare well with tabs, so i'm replacing these
        //             with spaces.
        return text.Replace("\t", "    ");
    }

    string GetDefaultSenderString(string sender)
    {
        if (!string.IsNullOrEmpty(sender))
            return $"<b>{sender}:</b>";

        return "";
    }

    DateTime UnixTimeStampToLocalDateTime(ulong unixTimeStampMilliseconds)
    {
        // TODO see if we can simplify with 'DateTimeOffset.FromUnixTimeMilliseconds'
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddMilliseconds(unixTimeStampMilliseconds).ToLocalTime();

        return dtDateTime;
    }
}
