using DCL.Helpers;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace DCL.Chat.Notifications
{
    public class ChatNotificationMessageComponentView :
        BaseComponentView,
        IChatNotificationMessageComponentView,
        IComponentModelConfig<ChatNotificationMessageComponentModel>,
        IShowableNotificationView
    {
        private const string NEAR_BY_CHANNEL = "~nearby";

        [Header("Prefab References")]
        [SerializeField] internal Button button;
        [SerializeField] public TMP_Text notificationMessage;
        [SerializeField] internal TMP_Text notificationHeader;
        [SerializeField] internal TMP_Text notificationSender;
        [SerializeField] internal TMP_Text notificationTimestamp;
        [SerializeField] internal ImageComponentView image;
        [SerializeField] internal GameObject imageContainer;
        [SerializeField] internal GameObject imageBackground;
        [SerializeField] internal GameObject multiNotificationBackground;
        [SerializeField] internal GameObject mentionMark;
        [SerializeField] internal bool isPrivate;
        [SerializeField] internal RectTransform backgroundTransform;
        [SerializeField] internal RectTransform messageContainerTransform;
        [SerializeField] internal bool isTopNorification;

        [Header("Configuration")]
        [SerializeField] internal ChatNotificationMessageComponentModel model;
        [SerializeField] private Color privateColor;
        [SerializeField] private Color publicColor;
        [SerializeField] private Color standardColor;
        [SerializeField] private Color[] channelColors;

        public event Action<string> OnClickedNotification;
        public bool shouldAnimateFocus = true;
        public string notificationTargetId;
        private int maxHeaderCharacters, maxSenderCharacters;
        private float startingXPosition;

        public void Configure(ChatNotificationMessageComponentModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public override void Awake()
        {
            base.Awake();
            button?.onClick.AddListener(() => OnClickedNotification?.Invoke(notificationTargetId));
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
            if (shouldAnimateFocus)
                messageContainerTransform.anchoredPosition = new Vector2(startingXPosition + 5, messageContainerTransform.anchoredPosition.y);
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();
            if (shouldAnimateFocus)
                messageContainerTransform.anchoredPosition = new Vector2(startingXPosition, messageContainerTransform.anchoredPosition.y);
        }

        public override void Hide(bool instant = false)
        {
            showHideAnimator.animSpeedFactor = 15f;
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
            SetImageVisibility(model.isImageVisible);
            SetOwnPlayerMention(model.isOwnPlayerMentioned);

            if (isTopNorification) return;

            if (model.isDockedLeft)
                DockLeft();
            else
                DockRight();
        }

        public void SetMessage(string message)
        {
            model.message = notificationMessage.ReplaceUnsupportedCharacters(message, '?');
            notificationMessage.text = message;

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
            if (!isPrivate)
            {
                if (header == NEAR_BY_CHANNEL)
                    notificationHeader.color = publicColor;
                else
                    SetRandomColorForChannel(header);
            }

            model.messageHeader = header;
            if (header.Length <= maxHeaderCharacters)
                notificationHeader.text = header;
            else
                notificationHeader.text = $"{header.Substring(0, maxHeaderCharacters)}...";

            ForceUIRefresh();
        }


        private void SetRandomColorForChannel(string header)
        {
            int seed = 0;
            byte[] ASCIIvalues = Encoding.ASCII.GetBytes(header);
            foreach (var value in ASCIIvalues)
            {
                seed += (int)value;
            }
            Random rand1 = new Random(seed);
            notificationHeader.color = channelColors[rand1.Next(0, channelColors.Length)];
        }

        public void SetNotificationSender(string sender)
        {
            model.messageSender = sender;
            if (sender.Length <= maxSenderCharacters)
                notificationSender.text = $"{sender}";
            else
                notificationSender.text = $"{sender.Substring(0, maxSenderCharacters)}:";

            ForceUIRefresh();
        }

        public void SetIsMultipleNotifications()
        {
            imageBackground.SetActive(false);
            multiNotificationBackground.SetActive(true);
            notificationHeader.color = standardColor;
            ForceUIRefresh();
        }

        public void SetIsPrivate(bool isPrivate)
        {
            model.isPrivate = isPrivate;
            this.isPrivate = isPrivate;
            imageBackground.SetActive(isPrivate);
            if (multiNotificationBackground != null)
                multiNotificationBackground.SetActive(false);

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

            image.SetImage((Sprite)null);
            model.imageUri = uri;
            image.SetImage(uri);
            ForceUIRefresh();
        }

        public void SetImageVisibility(bool visible)
        {
            model.isImageVisible = visible;
            imageContainer.SetActive(visible);
        }

        public void SetMaxContentCharacters(int maxContentCharacters)
        {
            maxContentCharacters = Mathf.Max(0, maxContentCharacters);
            model.maxContentCharacters = maxContentCharacters;
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

        public void SetOwnPlayerMention(bool isMentioned)
        {
            model.isOwnPlayerMentioned = isMentioned;

            if (mentionMark != null)
                mentionMark.SetActive(isMentioned);
        }

        public void DockRight()
        {
            model.isDockedLeft = false;
            DockHorizontally(1f);
        }

        public void DockLeft()
        {
            model.isDockedLeft = true;
            DockHorizontally(0);
        }

        private void DockHorizontally(float anchor)
        {
            messageContainerTransform.pivot = new Vector2(anchor, 0.5f);
            messageContainerTransform.anchorMin = new Vector2(anchor, 0.5f);
            messageContainerTransform.anchorMax = new Vector2(anchor, 0.5f);
            backgroundTransform.pivot = new Vector2(anchor, 0.5f);
        }

        private void ForceUIRefresh() =>
            backgroundTransform.ForceUpdateLayout();
    }
}
