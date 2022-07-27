using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatDescription : MonoBehaviour
{

    [SerializeField] private ChatHUDView chatView;
    [SerializeField] private bool analyzeResetPositionOnViewRefresh;
    private Vector3 originalAnchoredPosition;
    private Transform originalParent;
    private RectTransform myRectTransform;
    internal bool isParentedToChatContainer;

    private void Start()
    {
        myRectTransform = GetComponent<RectTransform>();
        chatView.OnChatContainerResized += AnalyzeRepositioning;
        if (analyzeResetPositionOnViewRefresh)
        {
            chatView.OnChatContainerResized += AnalyzeResetRepositioning;
            originalAnchoredPosition = myRectTransform.anchoredPosition;
            originalParent = myRectTransform.parent;
        }
    }
    
    private void AnalyzeResetRepositioning(Vector2 chatContainerRelativePositionToUpperLimit)
    {
        if (isParentedToChatContainer && chatContainerRelativePositionToUpperLimit.y < -myRectTransform.sizeDelta.y)
        {
            transform.SetParent(originalParent);
            transform.SetAsLastSibling();
            myRectTransform.anchoredPosition = originalAnchoredPosition;
            chatView.OnChatContainerResized += AnalyzeRepositioning;
            chatView.OnChatEntriesSorted -= RepositionDescriptionContainer;
            isParentedToChatContainer = false;
        }
    }

    private void AnalyzeRepositioning(Vector2 chatContainerRelativePositionToUpperLimit)
    {
        if (chatContainerRelativePositionToUpperLimit.y > -myRectTransform.sizeDelta.y)
        {
            transform.SetParent(chatView.chatEntriesContainer);
            transform.SetAsFirstSibling();
            chatView.OnChatContainerResized -= AnalyzeRepositioning;
            chatView.OnChatEntriesSorted += RepositionDescriptionContainer;
            isParentedToChatContainer = true;
        }
    }
        
    private void RepositionDescriptionContainer()
    {
        transform.SetAsFirstSibling();
    }

    private void OnDestroy()
    {
        chatView.OnChatContainerResized -= AnalyzeRepositioning;
        chatView.OnChatEntriesSorted -= RepositionDescriptionContainer;
        chatView.OnChatContainerResized -= AnalyzeResetRepositioning;
    }
}
