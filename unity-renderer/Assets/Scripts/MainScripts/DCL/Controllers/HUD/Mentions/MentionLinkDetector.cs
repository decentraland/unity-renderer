using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Social.Chat.Mentions
{
    public class MentionLinkDetector : MonoBehaviour, IPointerClickHandler
    {
        private const string MENTION_URL_PREFIX = "mention://";

        public event Action OnOwnPlayerMentioned;

        [SerializeField] internal TMP_Text textComponent;
        [SerializeField] internal UserProfile ownUserProfile;

        internal bool isMentionsFeatureEnabled = true;
        internal string currentText;
        internal bool hasNoParseLabel;
        private UserContextMenu contextMenu;
        private readonly CancellationTokenSource cancellationToken = new ();

        private void Awake()
        {
            if (textComponent == null)
                return;

            textComponent.OnPreRenderText += OnTextComponentPreRenderText;
        }

        private void Start()
        {
            isMentionsFeatureEnabled = DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("chat_mentions_enabled");
            DataStore.i.featureFlags.flags.OnChange += OnFeatureFlagsChanged;
        }

        private void OnDestroy()
        {
            if (textComponent == null)
                return;

            textComponent.OnPreRenderText -= OnTextComponentPreRenderText;
            DataStore.i.featureFlags.flags.OnChange -= OnFeatureFlagsChanged;

            cancellationToken.SafeCancelAndDispose();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isMentionsFeatureEnabled)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            string userName = GetUserNameByPointerPosition(eventData.position);
            if (string.IsNullOrEmpty(userName))
                return;

            ShowContextMenu(userName);
        }

        public void SetContextMenu(UserContextMenu userContextMenu)
        {
            this.contextMenu = userContextMenu;
        }

        private string GetUserNameByPointerPosition(Vector2 pointerPosition)
        {
            if (textComponent == null)
                return null;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, pointerPosition, textComponent.canvas.worldCamera);
            if (linkIndex == -1)
                return null;

            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];

            string mentionText = linkInfo.GetLinkText();
            string mentionLink = linkInfo.GetLinkID();

            return !MentionsUtils.IsAMention(mentionText)
                ? null
                : mentionLink.Replace(MENTION_URL_PREFIX, string.Empty);
        }

        private void ShowContextMenu(string userName)
        {
            if (contextMenu == null)
                return;

            var menuTransform = (RectTransform)contextMenu.transform;
            menuTransform.position = textComponent.transform.position;
            contextMenu.ShowByUserName(userName);
        }

        private void OnTextComponentPreRenderText(TMP_TextInfo textInfo)
        {
            if (!isMentionsFeatureEnabled)
                return;

            if (textInfo.textComponent.text == currentText)
                return;

            hasNoParseLabel = textInfo.textComponent.text.Contains("<noparse>", StringComparison.OrdinalIgnoreCase);
            RefreshMentionPatterns(cancellationToken.Token).Forget();
            CheckOwnPlayerMentionAsync(textInfo.textComponent, cancellationToken.Token).Forget();
        }

        private async UniTask RefreshMentionPatterns(CancellationToken cancellationToken)
        {
            await UniTask.WaitForEndOfFrame(this, cancellationToken);

            textComponent.text = MentionsUtils.ReplaceMentionPattern(textComponent.text, mention =>
            {
                string mentionWithoutSymbol = mention[1..];

                return hasNoParseLabel
                    ? $"</noparse><link={MENTION_URL_PREFIX}{mentionWithoutSymbol}><color=#4886E3><u>{mention}</u></color></link><noparse>"
                    : $"<link={MENTION_URL_PREFIX}{mentionWithoutSymbol}><color=#4886E3><u>{mention}</u></color></link>";
            });

            currentText = textComponent.text;
        }

        private async UniTask CheckOwnPlayerMentionAsync(TMP_Text textComp, CancellationToken ct)
        {
            if (ownUserProfile == null)
                return;

            await UniTask.WaitForEndOfFrame(this, ct);

            if (MentionsUtils.IsUserMentionedInText(ownUserProfile.userName, textComp.text))
                OnOwnPlayerMentioned?.Invoke();
        }

        private void OnFeatureFlagsChanged(FeatureFlag current, FeatureFlag previous) =>
            isMentionsFeatureEnabled = current.IsFeatureEnabled("chat_mentions_enabled");
    }
}
