using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Chat.HUD.Mentions
{
    public class MentionLinkDetector : MonoBehaviour, IPointerClickHandler
    {
        private const string MENTION_URL_PREFIX = "mention://";
        private const string MENTION_PATTERN = "^@[a-zA-Z\\d]{3,15}(#[a-zA-Z\\d]{4})?$";
        private static readonly Regex MENTION_REGEX = new (MENTION_PATTERN);

        public event Action OnOwnPlayerMentioned;

        [SerializeField] internal TMP_Text textComponent;
        [SerializeField] internal UserProfile ownUserProfile;

        private bool isMentionsFeatureEnabled = true;
        private UserContextMenu contextMenu;
        private string currentText;

        private void Awake()
        {
            // TODO: Check mentions Feature Flag...
            isMentionsFeatureEnabled = true;

            if (textComponent == null)
                return;

            textComponent.OnPreRenderText += OnTextComponentPreRenderText;
        }

        private void OnDestroy()
        {
            if (textComponent == null)
                return;

            textComponent.OnPreRenderText -= OnTextComponentPreRenderText;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isMentionsFeatureEnabled)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            var clickedLink = GetMentionLinkByPointerPosition(eventData.position);
            if (clickedLink == null)
                return;

            ShowContextMenu(clickedLink.Value.userId);
        }

        public void SetContextMenu(UserContextMenu userContextMenu)
        {
            this.contextMenu = userContextMenu;
        }

        private (string userId, string mention)? GetMentionLinkByPointerPosition(Vector2 pointerPosition)
        {
            if (textComponent == null)
                return null;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, pointerPosition, textComponent.canvas.worldCamera);
            if (linkIndex == -1)
                return null;

            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];

            string mentionText = linkInfo.GetLinkText();
            string mentionLink = linkInfo.GetLinkID();
            if (!IsAMention(mentionText, mentionLink))
                return null;

            return (
                mentionLink.Replace(MENTION_URL_PREFIX, string.Empty).ToLower(),
                mentionText
            );
        }

        private static bool IsAMention(string text, string url)
        {
            var match = MENTION_REGEX.Match(text);
            return match.Success && url.StartsWith(MENTION_URL_PREFIX);
        }

        private void ShowContextMenu(string userId)
        {
            if (contextMenu == null)
                return;

            var menuTransform = (RectTransform)contextMenu.transform;
            menuTransform.position = textComponent.transform.position;
            contextMenu.Show(userId);
        }

        private void OnTextComponentPreRenderText(TMP_TextInfo textInfo)
        {
            if (!isMentionsFeatureEnabled)
                return;

            if (textInfo.textComponent.text == currentText)
                return;

            if (ownUserProfile == null)
                return;

            // TODO: Put it into a UniTask and wait for WaitForEndOfFrame to throw the event
            if (textInfo.textComponent.text.ToLower().Contains($"{MENTION_URL_PREFIX}{ownUserProfile.userId.ToLower()}"))
                OnOwnPlayerMentioned?.Invoke();

            currentText = textInfo.textComponent.text;
        }
    }
}
