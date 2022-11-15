using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChannelLinkDetector : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] internal TMP_Text textComponent;

    internal string currentText;
    internal bool hasNoParseLabel;
    internal List<string> channelsFoundInText = new List<string>();
    private IChannelsFeatureFlagService channelsFeatureFlagService;
    private bool isAllowedToCreateChannels = true;

    private void Awake()
    {
        if (textComponent == null)
            return;

        textComponent.OnPreRenderText += OnTextComponentPreRenderText;
        
        if (Environment.i != null
            && Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>() != null)
        {
            channelsFeatureFlagService = Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>();
            isAllowedToCreateChannels = channelsFeatureFlagService.IsChannelsFeatureEnabled()
                                        && channelsFeatureFlagService.IsAllowedToCreateChannels();
            channelsFeatureFlagService.OnAllowedToCreateChannelsChanged += OnAllowedToCreateChannelsChanged;
        }
    }

    private void OnDestroy()
    {
        if (textComponent == null)
            return;

        textComponent.OnPreRenderText -= OnTextComponentPreRenderText;
        
        if (channelsFeatureFlagService != null)
            channelsFeatureFlagService.OnAllowedToCreateChannelsChanged -= OnAllowedToCreateChannelsChanged;
    }

    private void OnAllowedToCreateChannelsChanged(bool isAllowed) =>
        isAllowedToCreateChannels = isAllowed && channelsFeatureFlagService.IsChannelsFeatureEnabled();

    private void OnTextComponentPreRenderText(TMP_TextInfo textInfo)
    {
        if (currentText == textComponent.text) return;
        if (!isAllowedToCreateChannels) return;

        hasNoParseLabel = textInfo.textComponent.text.ToLower().Contains("<noparse>");
        CoroutineStarter.Start(RefreshChannelPatterns());
    }

    internal IEnumerator RefreshChannelPatterns()
    {
        yield return new WaitForEndOfFrame();

        channelsFoundInText = ChannelUtils.ExtractChannelIdsFromText(textComponent.text);

        foreach (var channelFound in channelsFoundInText)
        {
            textComponent.text = textComponent.text.Replace(
                channelFound,
                hasNoParseLabel ?
                    $"</noparse><link={channelFound}><color=#4886E3><u>{channelFound}</u></color></link><noparse>" :
                    $"<link={channelFound}><color=#4886E3><u>{channelFound}</u></color></link>");
        }

        currentText = textComponent.text;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isAllowedToCreateChannels) return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            string clickedLink = GetChannelLinkByPointerPosition(eventData.position);

            if (ChannelUtils.IsAChannel(clickedLink))
            {
                DataStore.i.channels.channelJoinedSource.Set(ChannelJoinedSource.Link);
                DataStore.i.channels.currentJoinChannelModal.Set(clickedLink.ToLower(), true);
            }
        }
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