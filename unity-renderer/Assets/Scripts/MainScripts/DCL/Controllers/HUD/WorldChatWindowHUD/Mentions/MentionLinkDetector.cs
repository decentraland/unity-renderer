using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Chat.HUD.Mentions
{
    public class MentionLinkDetector : MonoBehaviour, IPointerClickHandler
    {
        public event Action OnOwnPlayerMentioned;

        [SerializeField] internal TMP_Text textComponent;
        [SerializeField] internal UserProfile ownUserProfile;

        private bool isMentionsFeatureEnabled = true;
        private UserContextMenu contextMenu;
        private string currentText;
        private readonly CancellationTokenSource cancellationToken = new ();

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

            cancellationToken.SafeCancelAndDispose();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isMentionsFeatureEnabled)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            string userId = GetUserIdByPointerPosition(eventData.position);
            if (string.IsNullOrEmpty(userId))
                return;

            ShowContextMenu(userId);
        }

        public void SetContextMenu(UserContextMenu userContextMenu)
        {
            this.contextMenu = userContextMenu;
        }

        private string GetUserIdByPointerPosition(Vector2 pointerPosition)
        {
            if (textComponent == null)
                return null;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, pointerPosition, textComponent.canvas.worldCamera);
            if (linkIndex == -1)
                return null;

            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];

            string mentionText = linkInfo.GetLinkText();
            string mentionLink = linkInfo.GetLinkID();
            return !MentionsUtils.IsAMention(mentionText, mentionLink)
                ? null
                : MentionsUtils.GetUserIdFromMentionLink(mentionLink);
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

            currentText = textInfo.textComponent.text;
            CheckOwnPlayerMentionAsync(textInfo.textComponent, cancellationToken.Token).Forget();
        }

        private async UniTask CheckOwnPlayerMentionAsync(TMP_Text textComp, CancellationToken ct)
        {
            if (ownUserProfile == null)
                return;

            await UniTask.WaitForEndOfFrame(this, ct);

            if (MentionsUtils.IsUserMentionedInText(ownUserProfile.userId, textComp.text))
                OnOwnPlayerMentioned?.Invoke();
        }
    }
}
