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
        
        internal enum AvatarModifierAreaFeedbackState { NEVER_SHOWN, ICON_VISIBLE, WARNING_MESSAGE_VISIBLE }

        private const string PATH = "_AvatarModifierAreaFeedbackHUD";
        private const string PATH_TO_WARNING_MESSAGE = "_WarningMessageAreaFeedbackHUD";
        private BaseRefCounter<AvatarAreaWarningID> avatarAreaWarningsCounter;
        
        [SerializeField] private CanvasGroup warningMessageCanvasGroup;
        [SerializeField] private CanvasGroup warningIconCanvasGroup;
        [SerializeField] private RectTransform messageContainer;
        internal float animationDuration;
        internal bool isVisible;
        internal AvatarModifierAreaFeedbackState currentState;
        internal Dictionary<AvatarAreaWarningID, GameObject> warningMessagesDictionary;
        internal CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
        internal CancellationTokenSource deactivateIconAnimationToken = new CancellationTokenSource();
        internal CancellationTokenSource deactivateWarningMesageAnimationToken = new CancellationTokenSource();

        public static AvatarModifierAreaFeedbackView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<AvatarModifierAreaFeedbackView>(); }

        public void Awake()
        {
            animationDuration = 0.5f;
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
                GameObject newWarningMessage = Instantiate(Resources.Load<GameObject>(PATH_TO_WARNING_MESSAGE), messageContainer);
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
                ShowWarningMessage();
                HideFirstTimeWarningMessageUniTask(deactivatePreviewCancellationToken.Token).Forget();
            }
            else
            {
                ShowIcon();
            }
        }
        
        private void Hide()
        {
            isVisible = false;
            
            deactivatePreviewCancellationToken.Cancel();
            
            warningIconCanvasGroup.blocksRaycasts = false;
            
            if (currentState.Equals(AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE))
            {
                HideWarningMessage();
            }
            else
            {
                HideIcon();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isVisible) return;

            ShowWarningMessage();
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isVisible) return;
            
            HideWarningMessage();
            ShowIcon(true);
        }
        
        private void HideWarningMessage()
        {
            deactivateWarningMesageAnimationToken.Cancel();
            deactivateWarningMesageAnimationToken  = new CancellationTokenSource();
            WarningMessageAnimationUniTask(0, deactivateWarningMesageAnimationToken.Token).Forget();
        }
        
        private void ShowWarningMessage()
        {
            deactivateWarningMesageAnimationToken.Cancel();
            deactivateWarningMesageAnimationToken  = new CancellationTokenSource();
            WarningMessageAnimationUniTask(1, deactivateWarningMesageAnimationToken.Token).Forget();
            HideIcon(true);
            currentState = AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE;
        }
        
        private void HideIcon(bool instant = false)
        {
            deactivateIconAnimationToken.Cancel();
            deactivateIconAnimationToken  = new CancellationTokenSource();
            IconAnimationUniTask(0, deactivateWarningMesageAnimationToken.Token, instant).Forget();
        }
        
        private void ShowIcon(bool instant=false)
        {
            currentState = AvatarModifierAreaFeedbackState.ICON_VISIBLE;
            
            warningIconCanvasGroup.blocksRaycasts = true;
            
            deactivateIconAnimationToken.Cancel();
            deactivateIconAnimationToken  = new CancellationTokenSource();
            IconAnimationUniTask(1, deactivateIconAnimationToken.Token, instant).Forget();;
        }
       
        async UniTaskVoid HideFirstTimeWarningMessageUniTask(CancellationToken cancellationToken)
        {
            await UniTask.Delay(5000, cancellationToken: cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            HideWarningMessage();
            ShowIcon(true);
        }

        async UniTaskVoid WarningMessageAnimationUniTask(float destinationAlpha, CancellationToken cancellationToken)
        {
            float startAlpha = warningMessageCanvasGroup.alpha;
            var t = 0f;

            while (t < animationDuration)
            {
                t += Time.deltaTime;

                warningMessageCanvasGroup.alpha = Mathf.Lerp(startAlpha, destinationAlpha, t / animationDuration);
                
                await UniTask.Yield();
                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            warningMessageCanvasGroup.alpha = destinationAlpha;
        }

        async UniTaskVoid IconAnimationUniTask(float destinationAlpha, CancellationToken cancellationToken, bool instant=false)
        {
            if (instant)
            {
                warningIconCanvasGroup.alpha = destinationAlpha;
                return;
            }
            
            var t = 0f;
            float startAlphaMessage = warningIconCanvasGroup.alpha;

            while (t < animationDuration)
            {
                t += Time.deltaTime;

                warningIconCanvasGroup.alpha = Mathf.Lerp(startAlphaMessage, destinationAlpha,  t / animationDuration);
                
                await UniTask.Yield();
                if (cancellationToken.IsCancellationRequested)
                    return;
            }
            
            warningIconCanvasGroup.alpha = destinationAlpha;
        }

        public void Dispose()
        {
            deactivatePreviewCancellationToken?.Cancel();
            deactivateWarningMesageAnimationToken?.Cancel();
            deactivateIconAnimationToken?.Cancel();
            deactivatePreviewCancellationToken?.Dispose();
            deactivateWarningMesageAnimationToken?.Dispose();
            deactivateIconAnimationToken?.Dispose();
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
                    return "\u2022  Passports can not be opened";
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
