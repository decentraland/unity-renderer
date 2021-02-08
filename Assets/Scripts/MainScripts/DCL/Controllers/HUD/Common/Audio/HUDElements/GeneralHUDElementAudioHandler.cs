using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralHUDElementAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    protected bool playHover = true, playClick = true, playRelease = true;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHover)
            return;

        if (!Input.GetMouseButton(0))
        {
            AudioScriptableObjects.buttonHover.Play(true);
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!playClick)
            return;

        AudioScriptableObjects.buttonClick.Play(true);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!playRelease)
            return;

        AudioScriptableObjects.buttonRelease.Play(true);
    }
}