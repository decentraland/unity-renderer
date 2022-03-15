using DCL.Helpers;
using DCL.Interface;
using System;
using DCL.SettingsCommon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DCL;

public class ChatEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private const string COORDINATES_COLOR_PRIVATE = "#4886E3";
    private const string COORDINATES_COLOR_PUBLIC = "#62C6FF";

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
        public string senderId;
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
    [SerializeField] internal float timeToHoverGotoPanel = 1f;

    [NonSerialized] public string messageLocalDateTime;

    bool fadeEnabled = false;
    double fadeoutStartTime;
    float hoverPanelTimer = 0;
    float hoverGotoPanelTimer = 0;
    bool isOverCoordinates = false;
    ParcelCoordinates currentCoordinates;

    public RectTransform hoverPanelPositionReference;
    public RectTransform contextMenuPositionReference;

    public Model model { get; private set; }

    public event Action<string> OnPress;
    public event Action<ChatEntry> OnPressRightButton;
    public event Action<ChatEntry> OnTriggerHover;
    public event Action<ChatEntry, ParcelCoordinates> OnTriggerHoverGoto;
    public event Action OnCancelHover;
    public event Action OnCancelGotoHover;

    private List<string> textCoords = new List<string>();

    public void Populate(Model chatEntryModel)
    {
        this.model = chatEntryModel;

        string userString = GetDefaultSenderString(chatEntryModel.senderName);

        if (chatEntryModel.subType == Model.SubType.PRIVATE_FROM)
        {
            userString = $"<b>From {chatEntryModel.senderName}:</b>";
        }
        else if (chatEntryModel.subType == Model.SubType.PRIVATE_TO)
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

        if (CoordinateUtils.HasValidTextCoordinates(body.text)) {
            List<string> textCoordinates = CoordinateUtils.GetTextCoordinates(body.text);
            for (int i = 0; i < textCoordinates.Count; i++)
            {
                PreloadSceneMetadata(CoordinateUtils.ParseCoordinatesString(textCoordinates[i]));
                string coordinatesColor;
                if (chatEntryModel.messageType == ChatMessage.Type.PRIVATE)
                    coordinatesColor = COORDINATES_COLOR_PRIVATE;
                else
                    coordinatesColor = COORDINATES_COLOR_PUBLIC;

                body.text = body.text.Replace(textCoordinates[i], $"</noparse><link={textCoordinates[i]}><color={coordinatesColor}><u>{textCoordinates[i]}</u></color></link><noparse>");
            }
        }

        messageLocalDateTime = UnixTimeStampToLocalDateTime(chatEntryModel.timestamp).ToString();

        Utils.ForceUpdateLayout(transform as RectTransform);

        if (fadeEnabled)
            group.alpha = 0;

        if (HUDAudioHandler.i != null)
        {
            // Check whether or not this message is new, and chat sounds are enabled in settings
            if (chatEntryModel.timestamp > HUDAudioHandler.i.chatLastCheckedTimestamp && Settings.i.audioSettings.Data.chatSFXEnabled)
            {
                switch (chatEntryModel.messageType)
                {
                    case ChatMessage.Type.PUBLIC:
                        // Check whether or not the message was sent by the local player
                        if (chatEntryModel.senderId == UserProfile.GetOwnUserProfile().userId)
                            AudioScriptableObjects.chatSend.Play(true);
                        else
                            AudioScriptableObjects.chatReceiveGlobal.Play(true);
                        break;
                    case ChatMessage.Type.PRIVATE:
                        switch (chatEntryModel.subType)
                        {
                            case Model.SubType.PRIVATE_FROM:
                                AudioScriptableObjects.chatReceivePrivate.Play(true);
                                break;
                            case Model.SubType.PRIVATE_TO:
                                AudioScriptableObjects.chatSend.Play(true);
                                break;
                            default:
                                break;
                        }
                        break;
                    case ChatMessage.Type.SYSTEM:
                        AudioScriptableObjects.chatReceiveGlobal.Play(true);
                        break;
                    default:
                        break;
                }
            }

            HUDAudioHandler.i.RefreshChatLastCheckedTimestamp();
        }
    }

    private void PreloadSceneMetadata(ParcelCoordinates parcelCoordinates)
    {
        if (MinimapMetadata.GetMetadata().GetSceneInfo(parcelCoordinates.x, parcelCoordinates.y) == null)
            WebInterface.RequestScenesInfoAroundParcel(new Vector2(parcelCoordinates.x, parcelCoordinates.y), 2);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(body, pointerEventData.position, null);
            if (linkIndex != -1)
            {
                DataStore.i.HUDs.gotoPanelVisible.Set(true);
                TMP_LinkInfo linkInfo = body.textInfo.linkInfo[linkIndex];
                ParcelCoordinates parcelCoordinate = CoordinateUtils.ParseCoordinatesString(linkInfo.GetLinkID().ToString());
                DataStore.i.HUDs.gotoPanelCoordinates.Set(parcelCoordinate);
            }

            if (model.messageType != ChatMessage.Type.PRIVATE)
                return;

            OnPress?.Invoke(model.otherUserId);
        }
        else if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            if ((model.messageType != ChatMessage.Type.PUBLIC && model.messageType != ChatMessage.Type.PRIVATE) ||
                model.senderId == UserProfile.GetOwnUserProfile().userId)
                return;

            OnPressRightButton?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData) 
    {
        if (pointerEventData == null)
            return;

        hoverPanelTimer = timeToHoverPanel;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (pointerEventData == null)
            return;

        hoverPanelTimer = 0f;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(body, pointerEventData.position, null);
        if (linkIndex == -1)
        {
            isOverCoordinates = false;
            hoverGotoPanelTimer = 0;
            OnCancelGotoHover?.Invoke();
        }
        OnCancelHover?.Invoke();
    }

    void OnDisable() { OnPointerExit(null); }

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
        CheckHoverCoordinates();
        ProcessHoverPanelTimer();
        ProcessHoverGotoPanelTimer();
    }

    private void CheckHoverCoordinates()
    {
        if (isOverCoordinates)
            return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(body, Input.mousePosition, null);

        if (linkIndex == -1)
            return;

        isOverCoordinates = true;
        TMP_LinkInfo linkInfo = body.textInfo.linkInfo[linkIndex];
        currentCoordinates = CoordinateUtils.ParseCoordinatesString(linkInfo.GetLinkID().ToString());
        hoverGotoPanelTimer = timeToHoverGotoPanel;
        OnCancelHover?.Invoke();
    }

    void Fade()
    {
        if (!fadeEnabled)
            return;

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
        if (hoverPanelTimer <= 0f || isOverCoordinates)
            return;

        hoverPanelTimer -= Time.deltaTime;
        if (hoverPanelTimer <= 0f)
        {
            hoverPanelTimer = 0f;

            OnTriggerHover?.Invoke(this);
        }
    }

    void ProcessHoverGotoPanelTimer()
    {
        if (hoverGotoPanelTimer <= 0f || !isOverCoordinates)
            return;

        hoverGotoPanelTimer -= Time.deltaTime;
        if (hoverGotoPanelTimer <= 0f)
        {
            hoverGotoPanelTimer = 0f;

            OnTriggerHoverGoto?.Invoke(this, currentCoordinates);
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