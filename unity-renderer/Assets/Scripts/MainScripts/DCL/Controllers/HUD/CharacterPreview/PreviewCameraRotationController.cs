using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraRotationController
    {
        public event Action<float> OnHorizontalRotation;
        public event Action<double> OnEndDragEvent;

        private readonly InputAction_Hold firstClickAction;
        private readonly float rotationFactor;
        private readonly float slowDownTime;
        private readonly ICharacterPreviewInputDetector characterPreviewInputDetector;

        private float currentHorizontalRotationVelocity;
        private float slowDownVelocity;
        private CancellationTokenSource cts = new ();
        private float timer;
        private DateTime startDragDateTime;

        public PreviewCameraRotationController(
            InputAction_Hold firstClickAction,
            float rotationFactor,
            float slowDownTime,
            ICharacterPreviewInputDetector characterPreviewInputDetector)
        {
            this.firstClickAction = firstClickAction;
            this.rotationFactor = rotationFactor;
            this.slowDownTime = slowDownTime;
            this.characterPreviewInputDetector = characterPreviewInputDetector;

            characterPreviewInputDetector.OnDragStarted += OnBeginDrag;
            characterPreviewInputDetector.OnDragging += OnDrag;
            characterPreviewInputDetector.OnDragFinished += OnEndDrag;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!firstClickAction.isOn)
                return;

            cts = cts.SafeRestart();
            startDragDateTime = DateTime.Now;
            AudioScriptableObjects.buttonClick.Play(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!firstClickAction.isOn)
                return;

            currentHorizontalRotationVelocity = rotationFactor * eventData.delta.x;
            OnHorizontalRotation?.Invoke(currentHorizontalRotationVelocity);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            timer = slowDownTime;
            slowDownVelocity = currentHorizontalRotationVelocity;
            SlowDownAsync(cts.Token).Forget();
            OnEndDragEvent?.Invoke((DateTime.Now - startDragDateTime).TotalMilliseconds);
            AudioScriptableObjects.buttonRelease.Play(true);
        }

        private async UniTask SlowDownAsync(CancellationToken ct)
        {
            float inverseTimer = 1f / slowDownTime;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                currentHorizontalRotationVelocity  = Mathf.Lerp(slowDownVelocity, 0, 1 - (timer * inverseTimer));
                OnHorizontalRotation?.Invoke(currentHorizontalRotationVelocity);
                await UniTask.NextFrame(ct);
            }
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            characterPreviewInputDetector.OnDragStarted -= OnBeginDrag;
            characterPreviewInputDetector.OnDragging -= OnDrag;
            characterPreviewInputDetector.OnDragFinished -= OnEndDrag;
        }
    }
}
