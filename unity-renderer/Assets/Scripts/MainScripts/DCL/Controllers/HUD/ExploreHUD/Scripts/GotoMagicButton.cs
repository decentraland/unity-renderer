using UnityEngine;
using UnityEngine.EventSystems;
using System;

internal class GotoMagicButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] Animator buttonAnimator;

    public event Action OnGotoMagicPressed, OnGotoMagicPointerEnter, onGotoMagicPointerExit;

    void OnEnable()
    {
        buttonAnimator.ResetTrigger("Highlighted");
        buttonAnimator.SetTrigger("Normal");
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        OnGotoMagicPressed?.Invoke();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        buttonAnimator.ResetTrigger("Normal");
        buttonAnimator.SetTrigger("Highlighted");

        OnGotoMagicPointerEnter?.Invoke();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        buttonAnimator.ResetTrigger("Highlighted");
        buttonAnimator.SetTrigger("Normal");

        onGotoMagicPointerExit?.Invoke();
    }
}
