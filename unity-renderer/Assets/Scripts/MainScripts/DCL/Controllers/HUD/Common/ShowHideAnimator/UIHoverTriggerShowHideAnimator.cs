using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class UIHoverTriggerShowHideAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] ShowHideAnimator showHideAnimator = null;
    [SerializeField] bool hideAnimatorOnAwake = false;
    [SerializeField] bool enableAnimatorOnHover = false;

    [Range(0, 10)]
    [SerializeField] float setVisibleDelay = 0;

    [Range(0, 10)]
    [SerializeField] float setHideDelay = 0;

    Coroutine delayRoutine = null;

    void Awake()
    {
        if (hideAnimatorOnAwake)
        {
            showHideAnimator.gameObject.SetActive(false);
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Show();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }

    IEnumerator DelayRoutine(float delay, Action callback)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        callback?.Invoke();
    }

    void Show()
    {
        StartDelayRoutine(setVisibleDelay, () =>
        {
            if (enableAnimatorOnHover && !showHideAnimator.gameObject.activeSelf)
            {
                showHideAnimator.gameObject.SetActive(true);
            }
            showHideAnimator.Show();
        });
    }

    void Hide()
    {
        StartDelayRoutine(setHideDelay, () =>
        {
            showHideAnimator.Hide();
        });
    }

    void StopDelayRoutine()
    {
        if (delayRoutine != null)
        {
            StopCoroutine(delayRoutine);
        }
    }

    void StartDelayRoutine(float delay, Action callback)
    {
        StopDelayRoutine();
        delayRoutine = StartCoroutine(DelayRoutine(delay, callback));
    }
}
