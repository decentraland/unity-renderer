using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAudioHandler : GeneralHUDElementAudioHandler
{
    protected Selectable selectable;
    [SerializeField]
    AudioEvent extraClickEvent = null;

    void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                base.OnPointerEnter(eventData);
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                base.OnPointerDown(eventData);
            }
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                base.OnPointerUp(eventData);

                if (extraClickEvent != null)
                    extraClickEvent.Play(true);
            }
        }
    }
}