using Cysharp.Threading.Tasks;
using DCL.Chat.Channels;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Social.Chat
{
    public class ChannelLinkDetector : MonoBehaviour, IPointerClickHandler, IChannelLinkDetectorView
    {
        public static readonly BaseHashSet<ChannelLinkDetector> INSTANCES = new ();

        [SerializeField] internal TMP_Text textComponent;

        private readonly CancellationTokenSource cancellationToken = new ();

        internal string currentText;
        internal bool hasNoParseLabel;
        internal List<string> channelsFoundInText = new ();

        public event Action<string> OnClicked;

        private void Awake()
        {
            INSTANCES.Add(this);
        }

        private void OnDestroy()
        {
            INSTANCES.Remove(this);

            cancellationToken.Cancel();
            cancellationToken.Dispose();

            if (textComponent == null)
                return;

            textComponent.OnPreRenderText -= OnTextComponentPreRenderText;
        }

        public void Enable()
        {
            if (textComponent == null) return;

            textComponent.OnPreRenderText -= OnTextComponentPreRenderText;
            textComponent.OnPreRenderText += OnTextComponentPreRenderText;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            string clickedLink = GetChannelLinkByPointerPosition(eventData.position);
            OnClicked?.Invoke(clickedLink);
        }

        private void OnTextComponentPreRenderText(TMP_TextInfo textInfo)
        {
            if (currentText == textComponent.text) return;

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
