using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Vector3 = UnityEngine.Vector3;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraPanning : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public event Action<Vector3> OnPanning;

        public float panSpeed = 0.2f;
        public bool allowVerticalPanning = true;
        public bool allowHorizontalPanning = true;
        public float inertiaDuration = 0.5f;

        private Vector3 lastMousePosition;
        private Vector3 lastPanningDeltaBeforeEndDrag;
        private Coroutine inertiaCoroutine;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Input.GetMouseButton(2))
                return;

            lastMousePosition = Input.mousePosition;
            AudioScriptableObjects.buttonClick.Play(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Input.GetMouseButton(2))
                return;

            if (inertiaCoroutine != null)
            {
                StopCoroutine(inertiaCoroutine);
                inertiaCoroutine = null;
            }

            var delta = Input.mousePosition - lastMousePosition;
            delta.y *= -1;

            if (!allowVerticalPanning)
                delta.y = 0f;

            if (!allowHorizontalPanning)
                delta.x = 0f;

            delta *= panSpeed * Time.deltaTime;
            lastPanningDeltaBeforeEndDrag = delta;
            lastMousePosition = Input.mousePosition;

            OnPanning?.Invoke(delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (lastPanningDeltaBeforeEndDrag.magnitude >= 0.01f)
                inertiaCoroutine ??= StartCoroutine(InertiaCoroutine());
            
            AudioScriptableObjects.buttonRelease.Play(true);
        }

        private IEnumerator InertiaCoroutine()
        {
            float inverseTimer = 1f / inertiaDuration;
            float timeLeft = inertiaDuration;

            while (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                OnPanning?.Invoke(Vector3.Lerp(lastPanningDeltaBeforeEndDrag, Vector3.zero, 1 - (timeLeft * inverseTimer)));
                yield return null;
            }
        }
    }
}
