using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraRotationController : IPreviewCameraRotationController
    {
        public event Action<float> OnHorizontalRotation;
        public event Action<double> OnEndDragEvent;

        private InputAction_Hold firstClickAction;
        private float rotationFactor;
        private float slowDownTime;
        private ICharacterPreviewInputDetector characterPreviewInputDetector;
        private float currentHorizontalRotationVelocity;
        private float slowDownVelocity;
        private CancellationTokenSource cts = new ();
        private float timer;
        private DateTime startDragDateTime;
        private Texture2D rotateCursorTexture;

        public void Configure(
            InputAction_Hold firstClickAction,
            float rotationFactor,
            float slowDownTime,
            ICharacterPreviewInputDetector characterPreviewInputDetector,
            Texture2D rotateCursorTexture)
        {
            this.firstClickAction = firstClickAction;
            this.rotationFactor = rotationFactor;
            this.slowDownTime = slowDownTime;
            this.characterPreviewInputDetector = characterPreviewInputDetector;
            this.rotateCursorTexture = rotateCursorTexture;

            characterPreviewInputDetector.OnDragStarted += OnBeginDrag;
            characterPreviewInputDetector.OnDragging += OnDrag;
            characterPreviewInputDetector.OnDragFinished += OnEndDrag;
        }

        private void OnBeginDrag(PointerEventData eventData)
        {
            if (!firstClickAction.isOn)
                return;

            cts = cts.SafeRestart();
            startDragDateTime = DateTime.Now;
            AudioScriptableObjects.buttonClick.Play(true);
        }

        private void OnDrag(PointerEventData eventData)
        {
            if (!firstClickAction.isOn)
                return;

            currentHorizontalRotationVelocity = rotationFactor * eventData.delta.x;
            OnHorizontalRotation?.Invoke(currentHorizontalRotationVelocity);

            if (rotateCursorTexture != null)
                Cursor.SetCursor(rotateCursorTexture, new Vector2(rotateCursorTexture.width / 2f, rotateCursorTexture.height / 2f), CursorMode.ForceSoftware);
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            timer = slowDownTime;
            slowDownVelocity = currentHorizontalRotationVelocity;
            SlowDownAsync(cts.Token).Forget();
            OnEndDragEvent?.Invoke((DateTime.Now - startDragDateTime).TotalMilliseconds);
            AudioScriptableObjects.buttonRelease.Play(true);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
