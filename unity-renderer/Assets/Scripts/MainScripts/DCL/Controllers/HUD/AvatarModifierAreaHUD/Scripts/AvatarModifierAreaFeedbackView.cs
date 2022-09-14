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
        private BaseRefCounter<AvatarModifierAreaID> avatarAreaWarningsCounter;
        private HUDCanvasCameraModeController hudCanvasCameraModeController;

        
        [SerializeField] internal RectTransform warningContainer;
        [SerializeField] private CanvasGroup pointerEnterTriggerArea;
        [SerializeField] private Animator messageAnimator;
       
        internal bool isVisible;
        internal AvatarModifierAreaFeedbackState currentState;
        internal Dictionary<AvatarModifierAreaID, GameObject> warningMessagesDictionary;
        internal CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();

        private string msgOutAnimationTrigger = "MsgOut";
        private string msgInAnimationTrigger = "MsgIn";
        private string iconInAnimationTrigger = "IconIn";
        private string iconOutAnimationTrigger = "IconOut";
        
        public static AvatarModifierAreaFeedbackView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<AvatarModifierAreaFeedbackView>(); }

        public void Awake()
        {
            currentState = AvatarModifierAreaFeedbackState.NEVER_SHOWN;
            hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);
        }

        public void SetUp(BaseRefCounter<AvatarModifierAreaID> avatarAreaWarnings)
        {
            avatarAreaWarningsCounter = avatarAreaWarnings;
            avatarAreaWarningsCounter.OnAdded += AddedNewWarning;
            avatarAreaWarningsCounter.OnRemoved += RemovedWarning;

            warningMessagesDictionary = new Dictionary<AvatarModifierAreaID, GameObject>();
            
            foreach (AvatarModifierAreaID warningMessageEnum in Enum.GetValues(typeof(AvatarModifierAreaID)))
            {
                GameObject newWarningMessage = Instantiate(Resources.Load<GameObject>(PATH_TO_WARNING_MESSAGE), warningContainer);
                newWarningMessage.GetComponent<TMP_Text>().text = GetWarningMessage(warningMessageEnum);
                newWarningMessage.SetActive(false);
                warningMessagesDictionary.Add(warningMessageEnum, newWarningMessage);
            }  
        }

        private void RemovedWarning(AvatarModifierAreaID obj)
        {
            warningMessagesDictionary[obj].gameObject.SetActive(false);
            if (avatarAreaWarningsCounter.Count().Equals(0))
            {
                Hide();
            }
        }
        private void AddedNewWarning(AvatarModifierAreaID obj)
        {
            warningMessagesDictionary[obj].gameObject.SetActive(true);
            Show();
        }

        private void Show()
        {
            if (isVisible) return;
            isVisible = true;
            ResetAllTriggers();
            
            if (currentState.Equals(AvatarModifierAreaFeedbackState.NEVER_SHOWN))
            {
                messageAnimator.SetTrigger(msgInAnimationTrigger);
                HideFirstTimeWarningMessageUniTask(deactivatePreviewCancellationToken.Token).Forget();
                currentState = AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE;
            }
            else
            {
                messageAnimator.SetTrigger(iconInAnimationTrigger);
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
                messageAnimator.SetTrigger(msgOutAnimationTrigger);
            }
            else
            {
                messageAnimator.SetTrigger(iconOutAnimationTrigger);
            }
            currentState = AvatarModifierAreaFeedbackState.NONE_VISIBLE;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isVisible) return;

            messageAnimator.SetTrigger(msgInAnimationTrigger);
            currentState = AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE;
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isVisible) return;
            
            messageAnimator.SetTrigger(iconInAnimationTrigger);
            currentState = AvatarModifierAreaFeedbackState.ICON_VISIBLE;
        }
        
      
        async UniTaskVoid HideFirstTimeWarningMessageUniTask(CancellationToken cancellationToken)
        {
            await UniTask.Delay(5000, cancellationToken: cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            messageAnimator.SetTrigger(iconInAnimationTrigger);
            
            currentState = AvatarModifierAreaFeedbackState.ICON_VISIBLE;
            pointerEnterTriggerArea.blocksRaycasts = true;
        }

        public void Dispose()
        {
            deactivatePreviewCancellationToken?.Cancel();
            deactivatePreviewCancellationToken?.Dispose();
            avatarAreaWarningsCounter.OnAdded -= AddedNewWarning;
            avatarAreaWarningsCounter.OnRemoved -= RemovedWarning;
			hudCanvasCameraModeController?.Dispose();
        }
        
        private string GetWarningMessage(AvatarModifierAreaID idToSet)
        {
            switch (idToSet)
            {
                case AvatarModifierAreaID.HIDE_AVATAR:
                    return "\u2022  The avatars are hidden";
                case AvatarModifierAreaID.DISABLE_PASSPORT:
                    return "\u2022  Your passport is disable for\n    other players";
                default:
                    throw new NotImplementedException();
            }
        }

        private void ResetAllTriggers()
        {
            messageAnimator.ResetTrigger(msgOutAnimationTrigger);
            messageAnimator.ResetTrigger(msgInAnimationTrigger);
            messageAnimator.ResetTrigger(iconInAnimationTrigger);
            messageAnimator.ResetTrigger(iconOutAnimationTrigger);
        }

        
    }
}
