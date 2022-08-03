using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackView : MonoBehaviour, IAvatarModifierAreaFeedbackView, IPointerEnterHandler, IPointerExitHandler
    {
        
        internal enum AvatarModifierAreaFeedbackState { NEVER_SHOWN, ICON_VISIBLE, WARNING_MESSAGE_VISIBLE, NONE_VISIBLE }

        private const string PATH = "_AvatarModifierAreaFeedbackHUD";
        private const string PATH_TO_WARNING_MESSAGE = "_WarningMessageAreaFeedbackHUD";
        private BaseRefCounter<AvatarAreaWarningID> avatarAreaWarningsCounter;
        
        [SerializeField] internal RectTransform warningContainer;
        [SerializeField] private CanvasGroup pointerEnterTriggerArea;
        [SerializeField] private Animator messageAnimator;
        [SerializeField] private Animator iconAnimator;
       
        internal bool isVisible;
        internal AvatarModifierAreaFeedbackState currentState;
        internal Dictionary<AvatarAreaWarningID, GameObject> warningMessagesDictionary;
        internal CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();

        private string outAnimationTrigger = "Out";
        private string inAnimationTrigger = "In";
        private string firstInAnimationTrigger = "FirstIn";
        public static AvatarModifierAreaFeedbackView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<AvatarModifierAreaFeedbackView>(); }

        public void Awake()
        {
            currentState = AvatarModifierAreaFeedbackState.NEVER_SHOWN;
        }

        public void SetUp(BaseRefCounter<AvatarAreaWarningID> avatarAreaWarnings)
        {
            avatarAreaWarningsCounter = avatarAreaWarnings;
            avatarAreaWarningsCounter.OnAdded += AddedNewWarning;
            avatarAreaWarningsCounter.OnRemoved += RemovedWarning;

            warningMessagesDictionary = new Dictionary<AvatarAreaWarningID, GameObject>();
            
            foreach (AvatarAreaWarningID warningMessageEnum in Enum.GetValues(typeof(AvatarAreaWarningID)))
            {
                GameObject newWarningMessage = Instantiate(Resources.Load<GameObject>(PATH_TO_WARNING_MESSAGE), warningContainer);
                newWarningMessage.GetComponent<TMP_Text>().text = GetWarningMessage(warningMessageEnum);
                newWarningMessage.SetActive(false);
                warningMessagesDictionary.Add(warningMessageEnum, newWarningMessage);
            }  
        }

        private void RemovedWarning(AvatarAreaWarningID obj)
        {
            warningMessagesDictionary[obj].gameObject.SetActive(false);
            if (avatarAreaWarningsCounter.Count().Equals(0))
            {
                Hide();
            }
        }
        private void AddedNewWarning(AvatarAreaWarningID obj)
        {
            warningMessagesDictionary[obj].gameObject.SetActive(true);
            Show();
        }

        private void Show()
        {
            if (isVisible) return;
            isVisible = true;
            
            if (currentState.Equals(AvatarModifierAreaFeedbackState.NEVER_SHOWN))
            {
                messageAnimator.SetTrigger(firstInAnimationTrigger);
                HideFirstTimeWarningMessageUniTask(deactivatePreviewCancellationToken.Token).Forget();
                currentState = AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE;
            }
            else
            {
                iconAnimator.SetTrigger(inAnimationTrigger);
                pointerEnterTriggerArea.blocksRaycasts = true;
                currentState = AvatarModifierAreaFeedbackState.ICON_VISIBLE;
            }
        }
        
        private void Hide()
        {
            isVisible = false;
            
            deactivatePreviewCancellationToken.Cancel();
            
            pointerEnterTriggerArea.blocksRaycasts = false;
            
            if (currentState.Equals(AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE))
            {
                messageAnimator.SetTrigger(outAnimationTrigger);
            }
            else
            {
                iconAnimator.SetTrigger(outAnimationTrigger);
            }
            currentState = AvatarModifierAreaFeedbackState.NONE_VISIBLE;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isVisible) return;

            iconAnimator.SetTrigger(outAnimationTrigger);
            messageAnimator.SetTrigger(inAnimationTrigger);
            currentState = AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE;
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isVisible) return;
            
            iconAnimator.SetTrigger(inAnimationTrigger);
            messageAnimator.SetTrigger(outAnimationTrigger);
            currentState = AvatarModifierAreaFeedbackState.ICON_VISIBLE;
        }
        
      
        async UniTaskVoid HideFirstTimeWarningMessageUniTask(CancellationToken cancellationToken)
        {
            await UniTask.Delay(5000, cancellationToken: cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            messageAnimator.SetTrigger(outAnimationTrigger);
            iconAnimator.SetTrigger(inAnimationTrigger);
            
            currentState = AvatarModifierAreaFeedbackState.ICON_VISIBLE;
            pointerEnterTriggerArea.blocksRaycasts = true;
        }

        public void Dispose()
        {
            deactivatePreviewCancellationToken?.Cancel();
            deactivatePreviewCancellationToken?.Dispose();
            avatarAreaWarningsCounter.OnAdded -= AddedNewWarning;
            avatarAreaWarningsCounter.OnRemoved -= RemovedWarning;
        }
        
        private string GetWarningMessage(AvatarAreaWarningID idToSet)
        {
            switch (idToSet)
            {
                case AvatarAreaWarningID.HIDE_AVATAR:
                    return "\u2022  The avatars are hidden";
                case AvatarAreaWarningID.DISABLE_PASSPORT:
                    return "\u2022  Your passport is disable for\n    other players";
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
