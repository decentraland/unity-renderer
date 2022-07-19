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
        void SetWarningMessage(List<IAvatarModifier> warningMessages);
        void ResetWarningMessage();

    }
    
    public class AvatarModifierAreaFeedbackView : MonoBehaviour, IAvatarModifierAreaFeedbackView, IPointerEnterHandler, IPointerExitHandler
    {

        private const string PATH = "_AvatarModifierAreaFeedbackHUD";

        internal enum AvatarModifierAreaFeedbackState { NEVER_SHOWN, ICON_VISIBLE, WARNING_MESSAGE_VISIBLE }

        [SerializeField] private RectTransform warningMessageMask;
        [SerializeField] private CanvasGroup warningIconCanvasGroup;
        [SerializeField] internal TMP_Text descriptionText;
        internal float animationDuration;
        internal bool isVisible;
        internal AvatarModifierAreaFeedbackState currentState;
        internal CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
        internal CancellationTokenSource deactivateIconAnimationToken = new CancellationTokenSource();
        internal CancellationTokenSource deactivateWarningMesageAnimationToken = new CancellationTokenSource();



        public static AvatarModifierAreaFeedbackView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<AvatarModifierAreaFeedbackView>(); }

        public void Awake()
        {
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
            
            warningIconCanvasGroup.blocksRaycasts = false;
            
            if (currentState.Equals(AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE))
            {
                HideWarningMessage();
                HideIcon(true);
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
            WarningMessageAnimationUniTask(Vector3.zero, deactivateWarningMesageAnimationToken.Token).Forget();
        }
        
        private void ShowWarningMessage()
        {
            deactivateWarningMesageAnimationToken.Cancel();
            deactivateWarningMesageAnimationToken  = new CancellationTokenSource();
            WarningMessageAnimationUniTask(new Vector2(267,132), deactivateWarningMesageAnimationToken.Token).Forget();
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

        public void SetWarningMessage(List<IAvatarModifier> newAvatarModifiers)
        {
            string newTextToSet = "";
            foreach (IAvatarModifier newAvatarModifierWarning in newAvatarModifiers)
            {
                newTextToSet += newAvatarModifierWarning.GetWarningDescription() + "\n";
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

        async UniTaskVoid WarningMessageAnimationUniTask(Vector2 destinationSizedelta, CancellationToken cancellationToken)
        {
            Vector2 startSizeDelta = warningMessageMask.sizeDelta;
            var t = 0f;

            while (t < animationDuration)
            {
                t += Time.deltaTime;

                warningMessageMask.sizeDelta = Vector2.Lerp(startSizeDelta, destinationSizedelta, t / animationDuration);
                
                await UniTask.Yield();
                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            warningMessageMask.sizeDelta = destinationSizedelta;
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
        }

    }
}
