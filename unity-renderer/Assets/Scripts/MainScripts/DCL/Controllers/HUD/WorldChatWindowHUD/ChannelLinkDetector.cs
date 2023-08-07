using Cysharp.Threading.Tasks;
using DCL.Chat;
using DCL.Chat.Channels;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Social.Chat
{
    public class ChannelLinkDetector : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] internal TMP_Text textComponent;

        private readonly CancellationTokenSource cancellationToken = new ();

        internal string currentText;
        internal bool hasNoParseLabel;
        internal List<string> channelsFoundInText = new ();
        private bool isAllowedToCreateChannels = true;

        private void Awake()
        {
            if (textComponent == null)
                return;

            textComponent.OnPreRenderText += OnTextComponentPreRenderText;

            if (Environment.i != null
                && Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>() != null)
            {
                var channelsFeatureFlagService = Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>();
                isAllowedToCreateChannels = channelsFeatureFlagService.IsChannelsFeatureEnabled();
            }
        }

        private void OnDestroy()
        {
            cancellationToken.Cancel();
            cancellationToken.Dispose();

            if (textComponent == null)
                return;

            textComponent.OnPreRenderText -= OnTextComponentPreRenderText;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isAllowedToCreateChannels) return;

            if (eventData.button != PointerEventData.InputButton.Left) return;
            string clickedLink = GetChannelLinkByPointerPosition(eventData.position);

            if (!ChannelUtils.IsAChannel(clickedLink)) return;
            DataStore.i.channels.channelJoinedSource.Set(ChannelJoinedSource.Link);
            DataStore.i.channels.currentJoinChannelModal.Set(clickedLink.ToLower(), true);
        }

        private void OnTextComponentPreRenderText(TMP_TextInfo textInfo)
        {
            if (currentText == textComponent.text) return;
            if (!isAllowedToCreateChannels) return;

            hasNoParseLabel = textInfo.textComponent.text.ToLower().Contains("<noparse>");
            RefreshChannelPatterns(cancellationToken.Token).Forget();
        }

        internal async UniTask RefreshChannelPatterns(CancellationToken cancellationToken)
        {
            await UniTask.WaitForEndOfFrame(this, cancellationToken);

            channelsFoundInText = ChannelUtils.ExtractChannelIdsFromText(textComponent.text);

            foreach (string channelFound in channelsFoundInText)
            {
                textComponent.text = textComponent.text.Replace(
                    channelFound,
                    hasNoParseLabel ?
                        $"</noparse><link={channelFound}><color=#4886E3><u>{channelFound}</u></color></link><noparse>" :
                        $"<link={channelFound}><color=#4886E3><u>{channelFound}</u></color></link>");
            }

            currentText = textComponent.text;
        }

        private string GetChannelLinkByPointerPosition(Vector2 pointerPosition)
        {
            if (textComponent == null)
                return "";

            string result = string.Empty;
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, pointerPosition, textComponent.canvas.worldCamera);

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
                result = linkInfo.GetLinkID();
            }

            return result;
        }
    }
}
