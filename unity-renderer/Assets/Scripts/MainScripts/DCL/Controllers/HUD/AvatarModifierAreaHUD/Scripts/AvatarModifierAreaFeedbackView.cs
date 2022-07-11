using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackView : BaseComponentView
    {

        private const string PATH = "_AvatarModifierAreaFeedbackHUD";

        private enum AvatarModifierAreaFeedbackState { NEVER_SHOWN, ICON_VISIBLE, WARNING_MESSAGE_VISIBLE }

        [SerializeField] private RectTransform warningMessageRectTransform;
        [SerializeField] private CanvasGroup warningIconCanvasGroup;
        [SerializeField] internal TMP_Text descriptionText;
        private CanvasGroup warningMessageGroup;
        private float animationDuration;
        private Coroutine warningMessageAnimationCoroutine;
        private Coroutine iconAnimationCoroutine;
        private AvatarModifierAreaFeedbackState currentState;
        private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();

        public static AvatarModifierAreaFeedbackView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<AvatarModifierAreaFeedbackView>(); }

        public override void Awake()
        {
            base.Awake();
            warningMessageGroup = warningMessageRectTransform.GetComponent<CanvasGroup>();
            animationDuration = 0.5f;
            currentState = AvatarModifierAreaFeedbackState.NEVER_SHOWN;
        }
        
        public override void Show(bool instant = false)
        {
            if (isVisible) return;
            
            base.Show(instant);
            if (currentState.Equals(AvatarModifierAreaFeedbackState.NEVER_SHOWN))
            {
                ShowWarningMessage();
                HideFirstTimeWarningMessage(deactivatePreviewCancellationToken.Token).Forget();
            }
            else
            {
                ShowIcon();
            }
        }
        
        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            
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
        
        public override void OnFocus()
        {
            base.OnFocus();
            
            if (!isVisible) return;

            ShowWarningMessage();
            HideIcon();
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();

            if (!isVisible) return;
            
            HideWarningMessage();
            ShowIcon();
        }
        
        private async UniTaskVoid HideFirstTimeWarningMessage(CancellationToken cancellationToken)
        {
            await UniTask.Delay(5000, cancellationToken: cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            HideWarningMessage();
            ShowIcon();
        }

        private void HideWarningMessage()
        {
            if (warningMessageAnimationCoroutine != null)
            {
                StopCoroutine(warningMessageAnimationCoroutine);
            }
            warningMessageAnimationCoroutine = StartCoroutine(WarningMessageAnimationCoroutine(Vector3.zero, 0));
        }
        
        private void ShowWarningMessage()
        {
            if (warningMessageAnimationCoroutine != null)
            {
                StopCoroutine(warningMessageAnimationCoroutine);
            }
            warningMessageAnimationCoroutine = StartCoroutine(WarningMessageAnimationCoroutine(Vector3.one, 1));
            currentState = AvatarModifierAreaFeedbackState.WARNING_MESSAGE_VISIBLE;
        }
        
        private void HideIcon()
        {
            if (iconAnimationCoroutine != null)
            {
                StopCoroutine(iconAnimationCoroutine);
            }
            iconAnimationCoroutine = StartCoroutine(IconAnimationCoroutine(0));
        }
        
        private void ShowIcon()
        {
            if (iconAnimationCoroutine != null)
            {
                StopCoroutine(iconAnimationCoroutine);
            }
            
            warningIconCanvasGroup.blocksRaycasts = true;
            
            iconAnimationCoroutine = StartCoroutine(IconAnimationCoroutine(1));
            currentState = AvatarModifierAreaFeedbackState.ICON_VISIBLE;
        }

        public void AddNewWarningMessage(string newWarning)
        {
            if (!string.IsNullOrEmpty(descriptionText.text)) descriptionText.text += "\n";
                
            descriptionText.text += newWarning;
        }

        public void ResetWarningMessage()
        {
            descriptionText.text = "";
        }

        public override void RefreshControl() { }

        IEnumerator WarningMessageAnimationCoroutine(Vector3 destinationScale, float destinationAlpha)
        {
            var t = 0f;
            Vector3 startScale = warningMessageRectTransform.localScale;
            float startAlphaMessage = warningMessageGroup.alpha;

            while (t < animationDuration)
            {
                t += Time.deltaTime;

                warningMessageRectTransform.localScale = Vector3.Lerp(startScale, destinationScale, t / animationDuration);
                warningMessageGroup.alpha = Mathf.Lerp(startAlphaMessage, destinationAlpha,  t / animationDuration);
                yield return null;
            }

            warningMessageRectTransform.localScale = destinationScale;
            warningMessageGroup.alpha = destinationAlpha;
        }

        IEnumerator IconAnimationCoroutine(float destinationAlpha)
        {
            var t = 0f;
            float startAlphaMessage = warningIconCanvasGroup.alpha;

            while (t < animationDuration)
            {
                t += Time.deltaTime;

                warningIconCanvasGroup.alpha = Mathf.Lerp(startAlphaMessage, destinationAlpha,  t / animationDuration);
                yield return null;
            }

            warningIconCanvasGroup.alpha = destinationAlpha;
        }


    }
}
