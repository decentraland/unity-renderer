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
    public interface IAvatarModifierAreaFeedbackView : IDisposable
    {
        void SetVisibility(bool visible);
        void SetWarningMessage(List<string> warningMessages);
        void ResetWarningMessage();

    }
    
    public class AvatarModifierAreaFeedbackView : MonoBehaviour, IAvatarModifierAreaFeedbackView, IPointerEnterHandler, IPointerExitHandler
    {

        private const string PATH = "_AvatarModifierAreaFeedbackHUD";

        internal enum AvatarModifierAreaFeedbackState { NEVER_SHOWN, ICON_VISIBLE, WARNING_MESSAGE_VISIBLE }

        [SerializeField] private RectTransform warningMessageRectTransform;
        [SerializeField] private CanvasGroup warningIconCanvasGroup;
        [SerializeField] internal TMP_Text descriptionText;
        internal CanvasGroup warningMessageGroup;
        internal float animationDuration;
        internal bool isVisible;
        internal AvatarModifierAreaFeedbackState currentState;
        internal CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
        internal CancellationTokenSource deactivateIconAnimationToken = new CancellationTokenSource();
        internal CancellationTokenSource deactivateWarningMesageAnimationToken = new CancellationTokenSource();



        public static AvatarModifierAreaFeedbackView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<AvatarModifierAreaFeedbackView>(); }

        public void Awake()
        {
            warningMessageGroup = warningMessageRectTransform.GetComponent<CanvasGroup>();
            animationDuration = 0.5f;
            currentState = AvatarModifierAreaFeedbackState.NEVER_SHOWN;
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
            deactivatePreviewCancellationToken = new CancellationTokenSource();
            
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
        
        public void SetVisibility(bool setVisible)
        {
            if (setVisible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isVisible) return;

            ShowWarningMessage();
            HideIcon();
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isVisible) return;
            
            HideWarningMessage();
            ShowIcon();
        }
        
        private void HideWarningMessage()
        {
            deactivateWarningMesageAnimationToken.Cancel();
            deactivateWarningMesageAnimationToken  = new CancellationTokenSource();
            WarningMessageAnimationUniTask(Vector3.zero, 0, deactivateWarningMesageAnimationToken.Token).Forget();
        }
        
        private void ShowWarningMessage()
        {
            deactivateWarningMesageAnimationToken.Cancel();
            deactivateWarningMesageAnimationToken  = new CancellationTokenSource();
            WarningMessageAnimationUniTask(Vector3.one, 1, deactivateWarningMesageAnimationToken.Token).Forget();
            currentState = AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE;
        }
        
        private void HideIcon()
        {
            deactivateIconAnimationToken.Cancel();
            deactivateWarningMesageAnimationToken  = new CancellationTokenSource();
            IconAnimationUniTask(0, deactivateWarningMesageAnimationToken.Token).Forget();
        }
        
        private void ShowIcon()
        {
            warningIconCanvasGroup.blocksRaycasts = true;
            deactivateIconAnimationToken.Cancel();
            deactivateWarningMesageAnimationToken  = new CancellationTokenSource();
            IconAnimationUniTask(1, deactivateWarningMesageAnimationToken.Token).Forget();;
            currentState = AvatarModifierAreaFeedbackState.ICON_VISIBLE;
        }

        public void SetWarningMessage(List<string> newAvatarModifiers)
        {
            string newTextToSet = "";
            foreach (string newAvatarModifierWarning in newAvatarModifiers)
            {
                newTextToSet += newAvatarModifierWarning + "\n";
            }

            descriptionText.text = newTextToSet;
        }
        public void ResetWarningMessage()
        {
            descriptionText.text = "";
        }
        
        async UniTaskVoid HideFirstTimeWarningMessageUniTask(CancellationToken cancellationToken)
        {
            await UniTask.Delay(5000, cancellationToken: cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            HideWarningMessage();
            ShowIcon();
        }

        async UniTaskVoid WarningMessageAnimationUniTask(Vector3 destinationScale, float destinationAlpha, CancellationToken cancellationToken)
        {
            var t = 0f;
            Vector3 startScale = warningMessageRectTransform.localScale;
            float startAlphaMessage = warningMessageGroup.alpha;

            while (t < animationDuration)
            {
                t += Time.deltaTime;

                warningMessageRectTransform.localScale = Vector3.Lerp(startScale, destinationScale, t / animationDuration);
                warningMessageGroup.alpha = Mathf.Lerp(startAlphaMessage, destinationAlpha,  t / animationDuration);
                await UniTask.Yield();
                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            warningMessageRectTransform.localScale = destinationScale;
            warningMessageGroup.alpha = destinationAlpha;
        }

        async UniTaskVoid IconAnimationUniTask(float destinationAlpha, CancellationToken cancellationToken)
        {
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
            deactivatePreviewCancellationToken?.Dispose();
            deactivateIconAnimationToken?.Dispose();
            deactivateWarningMesageAnimationToken?.Dispose();
        }

    }
}
