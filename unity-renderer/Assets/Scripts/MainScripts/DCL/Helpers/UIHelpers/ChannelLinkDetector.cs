using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class ChannelLinkDetector : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<string> OnChannelLinkHover;
    public event Action OnChannelLinkCancelHover;
    public event Action<string> OnChannelLinkClicked;

    private TMP_Text textComponent;
    private bool shouldCheckChannelHovering;
    private string lastHoveredChannel = string.Empty;
    private string currentText;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        textComponent.OnPreRenderText += OnTextComponentPreRenderText;
    }

    private void Update()
    {
        if (shouldCheckChannelHovering)
        {
            string channelHovered = GetChannelLinkByPointerPosition(Input.mousePosition);

            if (channelHovered != lastHoveredChannel)
            {
                lastHoveredChannel = channelHovered;

                if (!string.IsNullOrEmpty(channelHovered))
                    OnChannelLinkHover?.Invoke(channelHovered);
                else
                    OnChannelLinkCancelHover?.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        textComponent.OnPreRenderText -= OnTextComponentPreRenderText;
    }

    private void OnTextComponentPreRenderText(TMP_TextInfo textInfo)
    {
        if (currentText == textComponent.text)
            return;

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
            OnChannelLinkClicked?.Invoke(channelClicked);
    }

    public void OnPointerEnter(PointerEventData eventData) => shouldCheckChannelHovering = true;

    public void OnPointerExit(PointerEventData eventData) => shouldCheckChannelHovering = false;

    private string GetChannelLinkByPointerPosition(Vector2 pointerPosition)
    {
        string result = string.Empty;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, pointerPosition, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            result = linkInfo.GetLinkID();
        }

        return result;
    }
}