using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using Vector3 = UnityEngine.Vector3;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraPanningController : IPreviewCameraPanningController
    {
        public event Action<Vector3> OnPanning;

        private InputAction_Hold secondClickAction;
        private InputAction_Hold middleClickAction;
        private float panSpeed;
        private bool allowVerticalPanning;
        private bool allowHorizontalPanning;
        private float inertiaDuration;
        private ICharacterPreviewInputDetector characterPreviewInputDetector;
        private Vector3 lastMousePosition;
        private Vector3 lastPanningDeltaBeforeEndDrag;
        private CancellationTokenSource cts = new ();
        private Texture2D panningCursorTexture;

        public void Configure(
            InputAction_Hold secondClickAction,
            InputAction_Hold middleClickAction,
            float panSpeed,
            bool allowVerticalPanning,
            bool allowHorizontalPanning,
            float inertiaDuration,
            ICharacterPreviewInputDetector characterPreviewInputDetector,
            Texture2D panningCursorTexture)
        {
            this.secondClickAction = secondClickAction;
            this.middleClickAction = middleClickAction;
            this.panSpeed = panSpeed;
            this.allowVerticalPanning = allowVerticalPanning;
            this.allowHorizontalPanning = allowHorizontalPanning;
            this.inertiaDuration = inertiaDuration;
            this.characterPreviewInputDetector = characterPreviewInputDetector;
            this.panningCursorTexture = panningCursorTexture;

            characterPreviewInputDetector.OnDragStarted += OnBeginDrag;
            characterPreviewInputDetector.OnDragging += OnDrag;
            characterPreviewInputDetector.OnDragFinished += OnEndDrag;
        }

        private void OnBeginDrag(PointerEventData eventData)
        {
            if (!middleClickAction.isOn && !secondClickAction.isOn)
                return;

            cts = cts.SafeRestart();
            lastMousePosition = Input.mousePosition;
            AudioScriptableObjects.buttonClick.Play(true);
        }

        private void OnDrag(PointerEventData eventData)
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

            if (panningCursorTexture != null)
                Cursor.SetCursor(panningCursorTexture, new Vector2(panningCursorTexture.width / 2f, panningCursorTexture.height / 2f), CursorMode.ForceSoftware);
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            if (lastPanningDeltaBeforeEndDrag.magnitude >= 0.01f)
                InertiaAsync(cts.Token).Forget();

            AudioScriptableObjects.buttonRelease.Play(true);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            characterPreviewInputDetector.OnDragStarted -= OnBeginDrag;
            characterPreviewInputDetector.OnDragging -= OnDrag;
            characterPreviewInputDetector.OnDragFinished -= OnEndDrag;
        }
    }
}
