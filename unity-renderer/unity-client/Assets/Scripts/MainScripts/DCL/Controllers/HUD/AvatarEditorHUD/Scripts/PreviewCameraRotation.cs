using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewCameraRotation : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public event System.Action<float> OnHorizontalRotation;

    public float rotationFactor = -15f;

    public float slowDownTime = 0.5f;

    private float currentHorizontalRotationVelocity = 0f;

    private float slowDownVelocity;

    private Coroutine slowDownCoroutine;

    private float timer;

    public void OnBeginDrag(PointerEventData eventData)
    {
        AudioScriptableObjects.buttonClick.Play(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
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

        if (slowDownCoroutine == null)
        {
            slowDownCoroutine = StartCoroutine(SlowDown());
        }

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