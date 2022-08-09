using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChatDescription : BaseComponentView
    {

        [SerializeField] private ChatHUDView chatView;
        private RectTransform myRectTransform;
        private Vector3 originalAnchoredPosition;
        internal Transform originalParent;
        internal bool isParentedToChatContainer;

        public override void Start()
        {
            base.Start();
            myRectTransform = GetComponent<RectTransform>();
            originalAnchoredPosition = myRectTransform.anchoredPosition;
            originalParent = myRectTransform.parent;
            chatView.OnChatContainerResized += CheckIfReparentingIsRequired;
        }

        private async void CheckIfReparentingIsRequired()
        {
            //We need to wait one frame before checking the sizeDelta of the chatEntriesContainer. 
            //sizeDelta is not correctly resized on the same frame that this event is called.
            await UniTask.WaitForEndOfFrame();
        
            Vector2 chatContainerRelativePositionToUpperLimit = chatView.chatEntriesContainer.sizeDelta;
            if (!isParentedToChatContainer && chatContainerRelativePositionToUpperLimit.y > -myRectTransform.sizeDelta.y)
            {
                ReparentInScrollWhenItsFilled();
                return;
            }
        
            if (isParentedToChatContainer && chatContainerRelativePositionToUpperLimit.y < -myRectTransform.sizeDelta.y)
            {
                ReparentToOriginalPositionWhenScrollHasEmptySpace();
            }
        }

        private void ReparentInScrollWhenItsFilled()
        {
            transform.SetParent(chatView.chatEntriesContainer);
            transform.SetAsFirstSibling();
            chatView.OnChatEntriesSorted += RepositionDescriptionContainer;
            isParentedToChatContainer = true;
        }

        private void ReparentToOriginalPositionWhenScrollHasEmptySpace()
        {
            transform.SetParent(originalParent);
            transform.SetAsLastSibling();
            myRectTransform.anchoredPosition = originalAnchoredPosition;
            chatView.OnChatEntriesSorted -= RepositionDescriptionContainer;
            isParentedToChatContainer = false;
        }
        
        private void RepositionDescriptionContainer()
        {
            transform.SetAsFirstSibling();
        }
    
        public override void RefreshControl()
        {
        
        }

        private void OnDestroy()
        {
            chatView.OnChatContainerResized -= CheckIfReparentingIsRequired;
            chatView.OnChatEntriesSorted -= RepositionDescriptionContainer;
        }
    }
}