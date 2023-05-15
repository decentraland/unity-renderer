using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraRotation : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public event Action<float> OnHorizontalRotation;
        public event Action<double> OnEndDragEvent;

        public float rotationFactor = -15f;
        public float slowDownTime = 0.5f;

        private float currentHorizontalRotationVelocity;
        private float slowDownVelocity;
        private Coroutine slowDownCoroutine;
        private float timer;
        private DateTime startDragDateTime;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Input.GetMouseButton(0))
                return;

            startDragDateTime = DateTime.Now;
            AudioScriptableObjects.buttonClick.Play(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Input.GetMouseButton(0))
                return;

            if (slowDownCoroutine != null)
            {
                StopCoroutine(slowDownCoroutine);
                slowDownCoroutine = null;
            }

            currentHorizontalRotationVelocity = rotationFactor * eventData.delta.x;
            OnHorizontalRotation?.Invoke(currentHorizontalRotationVelocity);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            timer = slowDownTime;
            slowDownVelocity = currentHorizontalRotationVelocity;
            slowDownCoroutine ??= StartCoroutine(SlowDown());
            OnEndDragEvent?.Invoke((DateTime.Now - startDragDateTime).TotalMilliseconds);
            AudioScriptableObjects.buttonRelease.Play(true);
        }

        private IEnumerator SlowDown()
        {
            float inverseTimer = 1f / slowDownTime;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                currentHorizontalRotationVelocity  = Mathf.Lerp(slowDownVelocity, 0, 1 - (timer * inverseTimer));
                OnHorizontalRotation?.Invoke(currentHorizontalRotationVelocity);
                yield return null;
            }
        }
    }
}
