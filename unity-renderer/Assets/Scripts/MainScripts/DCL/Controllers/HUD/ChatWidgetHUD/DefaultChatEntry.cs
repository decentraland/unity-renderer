using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Interface;
using DCL.SettingsCommon;
using DCL.Social.Chat.Mentions;
using System;
using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCL.Chat.HUD
{
    public class DefaultChatEntry : ChatEntry, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public interface ILocalTimeConverterStrategy
        {
            DateTime GetLocalTime(ulong timestamp);
        }

        public class SystemLocalTimeConverterStrategy : ILocalTimeConverterStrategy
        {
            public DateTime GetLocalTime(ulong timestamp) =>
                DateTimeOffset.FromUnixTimeMilliseconds((long)timestamp)
                              .ToLocalTime()
                              .DateTime;
        }

        [SerializeField] internal TextMeshProUGUI body;
        [SerializeField] internal float timeToHoverPanel = 1f;
        [SerializeField] internal float timeToHoverGotoPanel = 1f;
        [SerializeField] internal bool showUserName = true;
        [SerializeField] private RectTransform hoverPanelPositionReference;
        [SerializeField] private RectTransform contextMenuPositionReference;
        [SerializeField] private MentionLinkDetector mentionLinkDetector;
        [SerializeField] private Color autoMentionBackgroundColor;
        [SerializeField] private Image backgroundImage;
        [SerializeField] internal UserProfile ownUserProfile;

        private float hoverPanelTimer;
        private float hoverGotoPanelTimer;
        private bool isOverCoordinates;
        private bool isShowingPreview;
        private ParcelCoordinates currentCoordinates;
        private ChatEntryModel model;
        private Color initialEntryColor;

        private readonly CancellationTokenSource populationTaskCancellationTokenSource = new CancellationTokenSource();

        public override ChatEntryModel Model => model;

        public ILocalTimeConverterStrategy LocalTimeConverterStrategy { get; set; } = new SystemLocalTimeConverterStrategy();

        public override string HoverString
        {
            get
            {
                var date = LocalTimeConverterStrategy.GetLocalTime(Model.timestamp)
                                                     .ToString("MM/dd/yy h:mm:ss tt", CultureInfo.InvariantCulture);

                return Model.isLoadingNames ? $"Loading name - {date}" : date;
            }
        }

        public override event Action<ChatEntry> OnUserNameClicked;
        public override event Action<ChatEntry> OnTriggerHover;
        public override event Action<ChatEntry, ParcelCoordinates> OnTriggerHoverGoto;
        public override event Action OnCancelHover;
        public override event Action OnCancelGotoHover;

        public void Awake()
        {
            initialEntryColor = backgroundImage.color;
        }

        public override void Populate(ChatEntryModel chatEntryModel) =>
            PopulateTask(chatEntryModel, populationTaskCancellationTokenSource.Token).Forget();

        private async UniTask PopulateTask(ChatEntryModel chatEntryModel, CancellationToken cancellationToken)
        {
            model = chatEntryModel;

            if(chatEntryModel.subType == ChatEntryModel.SubType.RECEIVED && chatEntryModel.messageType == ChatMessage.Type.PUBLIC)
                backgroundImage.color = initialEntryColor;

            chatEntryModel.bodyText = body.ReplaceUnsupportedCharacters(chatEntryModel.bodyText, '?');
            chatEntryModel.bodyText = RemoveTabs(chatEntryModel.bodyText);
            var userString = GetUserString(chatEntryModel);

            // Due to a TMPro bug in Unity 2020 LTS we have to wait several frames before setting the body.text to avoid a
            // client crash. More info at https://github.com/decentraland/unity-renderer/pull/2345#issuecomment-1155753538
            // TODO: Remove hack in a newer Unity/TMPro version
            await UniTask.NextFrame(cancellationToken);
            await UniTask.NextFrame(cancellationToken);
            await UniTask.NextFrame(cancellationToken);

            if (!string.IsNullOrEmpty(userString) && showUserName)
                body.text = $"{userString} {chatEntryModel.bodyText}";
            else
                body.text = chatEntryModel.bodyText;

            body.text = GetCoordinatesLink(body.text);

            (transform as RectTransform).ForceUpdateLayout();

            PlaySfx(chatEntryModel);
        }

        private string GetUserString(ChatEntryModel chatEntryModel)
        {
            if (string.IsNullOrEmpty(model.senderName)) return "";

            var baseName = model.senderName;

            switch (chatEntryModel.subType)
            {
                case ChatEntryModel.SubType.SENT:
                    switch (chatEntryModel.messageType)
                    {
                        case ChatMessage.Type.PUBLIC:
                        case ChatMessage.Type.PRIVATE when chatEntryModel.isChannelMessage:
                            baseName = "You";
                            break;
                        case ChatMessage.Type.PRIVATE:
                            baseName = $"To {chatEntryModel.recipientName}";
                            break;
                    }

                    break;
                case ChatEntryModel.SubType.RECEIVED:
                    switch (chatEntryModel.messageType)
                    {
                        case ChatMessage.Type.PRIVATE:
                            baseName = $"<color=#5EBD3D>From <link=username://{baseName}>{baseName}</link></color>";
                            break;
                        case ChatMessage.Type.PUBLIC:
                            baseName = $"<link=username://{baseName}>{baseName}</link>";
                            break;
                    }

                    break;
            }

            baseName = $"<b>{baseName}:</b>";

            return baseName;
        }

        private string GetCoordinatesLink(string body)
        {
            return CoordinateUtils.ReplaceTextCoordinates(body, (text, coordinates) =>
                $"</noparse><link={text}><color=#4886E3><u>{text}</u></color></link><noparse>");
        }

        private void PlaySfx(ChatEntryModel chatEntryModel)
        {
            if (HUDAudioHandler.i == null)
                return;

            bool areSoundsEnabled = Settings.i.audioSettings.Data.chatNotificationType == AudioSettings.ChatNotificationType.All;

            if (IsRecentMessage(chatEntryModel) && areSoundsEnabled)
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
                        }

                        break;
                    case ChatMessage.Type.SYSTEM:
                        AudioScriptableObjects.chatReceiveGlobal.Play(true);
                        break;
                }
            }

            HUDAudioHandler.i.RefreshChatLastCheckedTimestamp();
        }

        private bool IsRecentMessage(ChatEntryModel chatEntryModel)
        {
            return chatEntryModel.timestamp > HUDAudioHandler.i.chatLastCheckedTimestamp
                   && (DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds((long)chatEntryModel.timestamp))
                  .TotalSeconds < 30;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            if (pointerEventData.button != PointerEventData.InputButton.Left) return;

            int linkIndex =
                TMP_TextUtilities.FindIntersectingLink(body, pointerEventData.position, body.canvas.worldCamera);

            if (linkIndex == -1) return;

            string link = body.textInfo.linkInfo[linkIndex].GetLinkID();

            if (CoordinateUtils.HasValidTextCoordinates(link))
            {
                DataStore.i.HUDs.gotoPanelVisible.Set(true, true);
                var parcelCoordinate = CoordinateUtils.ParseCoordinatesString(link);
                DataStore.i.HUDs.gotoPanelCoordinates.Set(parcelCoordinate);
            }
            else if (link.StartsWith("username://"))
                OnUserNameClicked?.Invoke(this);
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

            var linkIndex =
                TMP_TextUtilities.FindIntersectingLink(body, pointerEventData.position,
                    DataStore.i.camera.hudsCamera.Get());

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

        private void OnDestroy()
        {
            populationTaskCancellationTokenSource.Cancel();

            if (mentionLinkDetector != null)
                mentionLinkDetector.OnOwnPlayerMentioned -= OnOwnPlayerMentioned;
        }

        public override void SetFadeout(bool enabled)
        {
            if (enabled) return;
            group.alpha = 1;
        }

        public override void DockContextMenu(RectTransform panel)
        {
            panel.pivot = new Vector2(0, 0);
            panel.position = contextMenuPositionReference.position;
        }

        public override void DockHoverPanel(RectTransform panel)
        {
            panel.pivot = hoverPanelPositionReference.pivot;
            panel.position = hoverPanelPositionReference.position;
        }

        public override void ConfigureMentionLinkDetector(UserContextMenu userContextMenu)
        {
            if (mentionLinkDetector == null)
                return;

            mentionLinkDetector.SetContextMenu(userContextMenu);
            mentionLinkDetector.OnOwnPlayerMentioned -= OnOwnPlayerMentioned;
            mentionLinkDetector.OnOwnPlayerMentioned += OnOwnPlayerMentioned;
        }

        private void Update()
        {
            // TODO: why it needs to be in an update? what about OnPointerEnter/OnPointerExit?
            CheckHoverCoordinates();
            ProcessHoverPanelTimer();
            ProcessHoverGotoPanelTimer();
        }

        private void CheckHoverCoordinates()
        {
            if (isOverCoordinates)
                return;

            var linkIndex =
                TMP_TextUtilities.FindIntersectingLink(body, Input.mousePosition, DataStore.i.camera.hudsCamera.Get());

            if (linkIndex == -1)
                return;

            var link = body.textInfo.linkInfo[linkIndex].GetLinkID();
            if (!CoordinateUtils.HasValidTextCoordinates(link)) return;

            isOverCoordinates = true;
            currentCoordinates = CoordinateUtils.ParseCoordinatesString(link);
            hoverGotoPanelTimer = timeToHoverGotoPanel;
            OnCancelHover?.Invoke();
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

        private void OnOwnPlayerMentioned()
        {
            if (model.senderId == ownUserProfile.userId)
                return;

            backgroundImage.color = autoMentionBackgroundColor;
        }
    }
}
