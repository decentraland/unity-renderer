using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraRotation : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public event Action<float> OnHorizontalRotation;
        public event Action<double> OnEndDragEvent;

        [SerializeField] internal InputAction_Hold firstClickAction;
        [SerializeField] internal float rotationFactor = -15f;
        [SerializeField] internal float slowDownTime = 0.5f;

        private float currentHorizontalRotationVelocity;
        private float slowDownVelocity;
        private CancellationTokenSource cts = new ();
        private float timer;
        private DateTime startDragDateTime;

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

        private void OnDestroy() =>
            cts.SafeCancelAndDispose();
    }
}
