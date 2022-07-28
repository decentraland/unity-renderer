using DCL;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class ChannelLinkDetector : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text textComponent;
    private string currentText;
    private bool hasNoParseLabel;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        textComponent.OnPreRenderText += OnTextComponentPreRenderText;
    }

    private void OnDestroy()
    {
        textComponent.OnPreRenderText -= OnTextComponentPreRenderText;
    }

    private void OnTextComponentPreRenderText(TMP_TextInfo textInfo)
    {
        if (currentText == textComponent.text)
            return;

        hasNoParseLabel = textInfo.textComponent.text.ToLower().Contains("<noparse>");
        CoroutineStarter.Start(RefreshChannelPatterns());
    }

    private IEnumerator RefreshChannelPatterns()
    {
        yield return new WaitForEndOfFrame();

        List<string> channelsFoundInText = ChannelUtils.ExtractChannelPatternsFromText(textComponent.text);

        foreach (var channelFound in channelsFoundInText)
        {
            textComponent.text = textComponent.text.Replace(
                channelFound,
                hasNoParseLabel ?
                    $"</noparse><link={channelFound}><color=#4886E3><u>{channelFound}</u></color></link><noparse>":
                    $"<link={channelFound}><color=#4886E3><u>{channelFound}</u></color></link>");
        }

        currentText = textComponent.text;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        string channelClicked = GetChannelLinkByPointerPosition(eventData.position);
        
        if (!string.IsNullOrEmpty(channelClicked))
            DataStore.i.channels.currentJoinChannelModal.Set(channelClicked, true);
    }

    private string GetChannelLinkByPointerPosition(Vector2 pointerPosition)
    {
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