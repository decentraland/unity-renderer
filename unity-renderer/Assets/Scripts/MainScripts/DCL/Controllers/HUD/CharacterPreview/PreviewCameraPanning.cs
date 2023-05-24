using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using Vector3 = UnityEngine.Vector3;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraPanning : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public event Action<Vector3> OnPanning;

        [SerializeField] internal InputAction_Hold secondClickAction;
        [SerializeField] internal InputAction_Hold middleClickAction;
        [SerializeField] internal float panSpeed = 0.2f;
        [SerializeField] internal bool allowVerticalPanning = true;
        [SerializeField] internal bool allowHorizontalPanning = true;
        [SerializeField] internal float inertiaDuration = 0.5f;

        private Vector3 lastMousePosition;
        private Vector3 lastPanningDeltaBeforeEndDrag;
        private CancellationTokenSource cts = new ();

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!middleClickAction.isOn && !secondClickAction.isOn)
                return;

            cts = cts.SafeRestart();
            lastMousePosition = Input.mousePosition;
            AudioScriptableObjects.buttonClick.Play(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!middleClickAction.isOn && !secondClickAction.isOn)
                return;

            var panningDelta = Input.mousePosition - lastMousePosition;
            panningDelta.y *= -1;

            if (!allowVerticalPanning)
                panningDelta.y = 0f;

            if (!allowHorizontalPanning)
                panningDelta.x = 0f;

            panningDelta *= panSpeed * Time.deltaTime;
            lastPanningDeltaBeforeEndDrag = panningDelta;
            lastMousePosition = Input.mousePosition;

            OnPanning?.Invoke(panningDelta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (lastPanningDeltaBeforeEndDrag.magnitude >= 0.01f)
                InertiaAsync(cts.Token).Forget();

            AudioScriptableObjects.buttonRelease.Play(true);
        }

        private async UniTask InertiaAsync(CancellationToken ct)
        {
            float inverseTimer = 1f / inertiaDuration;
            float timeLeft = inertiaDuration;

            while (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                OnPanning?.Invoke(Vector3.Lerp(lastPanningDeltaBeforeEndDrag, Vector3.zero, 1 - (timeLeft * inverseTimer)));
                await UniTask.NextFrame(ct);
            }
        }

        private void OnDestroy() =>
            cts.SafeCancelAndDispose();
    }
}
