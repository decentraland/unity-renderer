using System;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using DCL.SettingsCommon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultChatEntry : ChatEntry, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private const string COORDINATES_COLOR_PRIVATE = "#4886E3";
    private const string COORDINATES_COLOR_PUBLIC = "#62C6FF";

    [SerializeField] internal float timeToFade = 10f;
    [SerializeField] internal float fadeDuration = 5f;
    [SerializeField] internal TextMeshProUGUI body;
    [SerializeField] CanvasGroup group;
    [SerializeField] internal float timeToHoverPanel = 1f;
    [SerializeField] internal float timeToHoverGotoPanel = 1f;
    [SerializeField] private bool showUserName = true;
    [NonSerialized] public string messageLocalDateTime;

    private bool fadeEnabled;
    private double fadeoutStartTime;
    private float hoverPanelTimer;
    private float hoverGotoPanelTimer;
    private bool isOverCoordinates;
    private ParcelCoordinates currentCoordinates;
    private ChatEntryModel model;

    public RectTransform hoverPanelPositionReference;
    public RectTransform contextMenuPositionReference;

    public override ChatEntryModel Model => model;

    public event Action<string> OnPress;
    public event Action<DefaultChatEntry> OnPressRightButton;
    public event Action<DefaultChatEntry> OnTriggerHover;
    public event Action<DefaultChatEntry, ParcelCoordinates> OnTriggerHoverGoto;
    public event Action OnCancelHover;
    public event Action OnCancelGotoHover;

    public override void Populate(ChatEntryModel chatEntryModel)
    {
        model = chatEntryModel;

        string userString = GetDefaultSenderString(chatEntryModel.senderName);

        if (chatEntryModel.messageType == ChatMessage.Type.PRIVATE)
        {
            if (chatEntryModel.subType == ChatEntryModel.SubType.RECEIVED)
            {
                userString = $"<b>From {chatEntryModel.senderName}:</b>";
            }
            else if (chatEntryModel.subType == ChatEntryModel.SubType.SENT)
            {
                userString = $"<b>To {chatEntryModel.recipientName}:</b>";
            }
        }

        chatEntryModel.bodyText = RemoveTabs(chatEntryModel.bodyText);

        if (!string.IsNullOrEmpty(userString) && showUserName)
        {
            body.text = $"{userString} {chatEntryModel.bodyText}";
        }
        else
        {
            body.text = chatEntryModel.bodyText;
        }

        if (CoordinateUtils.HasValidTextCoordinates(body.text))
        {
            List<string> textCoordinates = CoordinateUtils.GetTextCoordinates(body.text);
            for (int i = 0; i < textCoordinates.Count; i++)
            {
                PreloadSceneMetadata(CoordinateUtils.ParseCoordinatesString(textCoordinates[i]));
                string coordinatesColor;
                if (chatEntryModel.messageType == ChatMessage.Type.PRIVATE)
                    coordinatesColor = COORDINATES_COLOR_PRIVATE;
                else
                    coordinatesColor = COORDINATES_COLOR_PUBLIC;

                body.text = body.text.Replace(textCoordinates[i],
                    $"</noparse><link={textCoordinates[i]}><color={coordinatesColor}><u>{textCoordinates[i]}</u></color></link><noparse>");
            }
        }

        messageLocalDateTime = UnixTimeStampToLocalDateTime(chatEntryModel.timestamp).ToString();

        Utils.ForceUpdateLayout(transform as RectTransform);

        if (fadeEnabled)
            group.alpha = 0;

        if (HUDAudioHandler.i != null)
        {
            // Check whether or not this message is new, and chat sounds are enabled in settings
            if (chatEntryModel.timestamp > HUDAudioHandler.i.chatLastCheckedTimestamp &&
                Settings.i.audioSettings.Data.chatSFXEnabled)
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
                            case ChatEntryModel.SubType.RECEIVED:
                                AudioScriptableObjects.chatReceivePrivate.Play(true);
                                break;
                            case ChatEntryModel.SubType.SENT:
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
                ParcelCoordinates parcelCoordinate =
                    CoordinateUtils.ParseCoordinatesString(linkInfo.GetLinkID().ToString());
                DataStore.i.HUDs.gotoPanelCoordinates.Set(parcelCoordinate);
            }

            if (Model.messageType != ChatMessage.Type.PRIVATE)
                return;

            OnPress?.Invoke(Model.otherUserId);
        }
        else if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            if ((Model.messageType != ChatMessage.Type.PUBLIC && Model.messageType != ChatMessage.Type.PRIVATE) ||
                Model.senderId == UserProfile.GetOwnUserProfile().userId)
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

    private void OnDisable()
    {
        OnPointerExit(null);
    }

    public override void SetFadeout(bool enabled)
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

    private void Update()
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

    private void Fade()
    {
        if (!fadeEnabled)
            return;

        //NOTE(Brian): Small offset using normalized Y so we keep the cascade effect
        double yOffset = (transform as RectTransform).anchoredPosition.y / (double) Screen.height * 2.0;

        double fadeTime = Math.Max(Model.timestamp / 1000.0, fadeoutStartTime) + timeToFade - yOffset;
        double currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        if (currentTime > fadeTime)
        {
            double timeSinceFadeTime = currentTime - fadeTime;
            group.alpha = Mathf.Clamp01(1 - (float) (timeSinceFadeTime / fadeDuration));
        }
        else
        {
            group.alpha += (1 - group.alpha) * 0.05f;
        }
    }

    private void ProcessHoverPanelTimer()
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

    private void ProcessHoverGotoPanelTimer()
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

    private string RemoveTabs(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        //NOTE(Brian): ContentSizeFitter doesn't fare well with tabs, so i'm replacing these
        //             with spaces.
        return text.Replace("\t", "    ");
    }

    private static string GetDefaultSenderString(string sender)
    {
        if (!string.IsNullOrEmpty(sender))
            return $"<b>{sender}:</b>";
        return "";
    }

    private static DateTime UnixTimeStampToLocalDateTime(ulong unixTimeStampMilliseconds)
    {
        // TODO see if we can simplify with 'DateTimeOffset.FromUnixTimeMilliseconds'
        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddMilliseconds(unixTimeStampMilliseconds).ToLocalTime();
        return dtDateTime;
    }
}