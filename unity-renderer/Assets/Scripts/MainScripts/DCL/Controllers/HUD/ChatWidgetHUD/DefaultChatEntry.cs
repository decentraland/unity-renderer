using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using DCL.SettingsCommon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DefaultChatEntry : ChatEntry, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] internal float timeToFade = 10f;
    [SerializeField] internal float fadeDuration = 5f;
    [SerializeField] internal TextMeshProUGUI body;
    [SerializeField] internal CanvasGroup group;
    [SerializeField] internal float timeToHoverPanel = 1f;
    [SerializeField] internal float timeToHoverGotoPanel = 1f;
    [SerializeField] internal bool showUserName = true;
    [SerializeField] private RectTransform hoverPanelPositionReference;
    [SerializeField] private RectTransform contextMenuPositionReference;
    [NonSerialized] public string messageLocalDateTime;

    [Header("Preview Mode")] [SerializeField]
    internal Image previewBackgroundImage;

    [SerializeField] internal Color previewBackgroundColor;
    [SerializeField] internal Color previewFontColor;

    private bool fadeEnabled;
    private double fadeoutStartTime;
    private float hoverPanelTimer;
    private float hoverGotoPanelTimer;
    private bool isOverCoordinates;
    private ParcelCoordinates currentCoordinates;
    private ChatEntryModel model;
    private Coroutine previewInterpolationRoutine;
    private Color originalBackgroundColor;
    private Color originalFontColor;

    public override ChatEntryModel Model => model;

    public event Action<string> OnPress;
    public event Action<DefaultChatEntry> OnPressRightButton;
    public event Action<DefaultChatEntry> OnTriggerHover;
    public event Action<DefaultChatEntry, ParcelCoordinates> OnTriggerHoverGoto;
    public event Action OnCancelHover;
    public event Action OnCancelGotoHover;

    private void Awake()
    {
        originalBackgroundColor = previewBackgroundImage.color;
        originalFontColor = body.color;
    }

    public override void Populate(ChatEntryModel chatEntryModel)
    {
        PopulateTask(chatEntryModel).Forget();
    }

    internal async UniTask PopulateTask(ChatEntryModel chatEntryModel)
    {
        // Due to a TMPro bug in Unity 2020 LTS we have to wait several frames before setting the body.text to avoid a
        // client crash. More info at https://github.com/decentraland/unity-renderer/pull/2345#issuecomment-1155753538
        await UniTask.NextFrame();
        await UniTask.NextFrame();
        await UniTask.NextFrame();
        
        model = chatEntryModel;

        chatEntryModel.bodyText = RemoveTabs(chatEntryModel.bodyText);
        var userString = GetUserString(chatEntryModel);
        
        if (!string.IsNullOrEmpty(userString) && showUserName)
            body.text = $"{userString} {chatEntryModel.bodyText}";
        else
            body.text = chatEntryModel.bodyText;

        body.text = GetCoordinatesLink(body.text);

        messageLocalDateTime = UnixTimeStampToLocalDateTime(chatEntryModel.timestamp).ToString();

        Utils.ForceUpdateLayout(transform as RectTransform);
        
        if (fadeEnabled)
            group.alpha = 0;

        PlaySfx(chatEntryModel);
    }

    private string GetUserString(ChatEntryModel chatEntryModel)
    {
        var userString = GetDefaultSenderString(chatEntryModel.senderName);
        
        if (chatEntryModel.messageType != ChatMessage.Type.PRIVATE) return userString;

        userString = chatEntryModel.subType switch
        {
            ChatEntryModel.SubType.RECEIVED => $"<b><color=#5EBD3D>From {chatEntryModel.senderName}:</color></b>",
            ChatEntryModel.SubType.SENT => $"<b>To {chatEntryModel.recipientName}:</b>",
            _ => userString
        };

        return userString;
    }

    private string GetCoordinatesLink(string body)
    {
        if (!CoordinateUtils.HasValidTextCoordinates(body)) return body;
        var textCoordinates = CoordinateUtils.GetTextCoordinates(body);

        for (var i = 0; i < textCoordinates.Count; i++)
        {
            // TODO: the preload should not be here
            PreloadSceneMetadata(CoordinateUtils.ParseCoordinatesString(textCoordinates[i]));

            body = body.Replace(textCoordinates[i],
                $"</noparse><link={textCoordinates[i]}><color=#4886E3><u>{textCoordinates[i]}</u></color></link><noparse>");
        }

        return body;
    }

    private void PlaySfx(ChatEntryModel chatEntryModel)
    {
        if (HUDAudioHandler.i == null) return;

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

    public override void DeactivatePreview()
    {
        if (!gameObject.activeInHierarchy)
        {
            previewBackgroundImage.color = originalBackgroundColor;
            body.color = originalFontColor;
            return;
        }

        if (previewInterpolationRoutine != null)
            StopCoroutine(previewInterpolationRoutine);

        previewInterpolationRoutine =
            StartCoroutine(InterpolatePreviewColor(originalBackgroundColor, originalFontColor, 0.5f));
    }

    public override void ActivatePreview()
    {
        if (!gameObject.activeInHierarchy)
        {
            ActivatePreviewInstantly();
            return;
        }

        if (previewInterpolationRoutine != null)
            StopCoroutine(previewInterpolationRoutine);

        previewInterpolationRoutine =
            StartCoroutine(InterpolatePreviewColor(previewBackgroundColor, previewFontColor, 0.5f));
    }

    public override void ActivatePreviewInstantly()
    {
        if (previewInterpolationRoutine != null)
            StopCoroutine(previewInterpolationRoutine);

        previewBackgroundImage.color = previewBackgroundColor;
        body.color = previewFontColor;
    }

    public override void DeactivatePreviewInstantly()
    {
        if (previewInterpolationRoutine != null)
            StopCoroutine(previewInterpolationRoutine);

        previewBackgroundImage.color = originalBackgroundColor;
        body.color = originalFontColor;
    }

    public void DockContextMenu(RectTransform panel)
    {
        panel.pivot = contextMenuPositionReference.pivot;
        panel.position = contextMenuPositionReference.position;
    }

    public void DockHoverPanel(RectTransform panel)
    {
        panel.pivot = hoverPanelPositionReference.pivot;
        panel.position = hoverPanelPositionReference.position;
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
        double yOffset = ((RectTransform) transform).anchoredPosition.y / (double) Screen.height * 2.0;
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

    private IEnumerator InterpolatePreviewColor(Color backgroundColor, Color fontColor, float duration)
    {
        var t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            previewBackgroundImage.color = Color.Lerp(previewBackgroundImage.color, backgroundColor, t / duration);
            body.color = Color.Lerp(body.color, fontColor, t / duration);

            yield return null;
        }

        previewBackgroundImage.color = backgroundColor;
        body.color = fontColor;
    }
}