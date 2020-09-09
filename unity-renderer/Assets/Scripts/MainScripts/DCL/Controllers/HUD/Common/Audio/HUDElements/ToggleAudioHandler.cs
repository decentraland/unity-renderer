using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleAudioHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (toggle != null)
        {
            if (toggle.interactable)
            {
                AudioScriptableObjects.buttonClick.Play(true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (toggle != null)
        {
            if (toggle.interactable)
            {
                if (toggle.isOn)
                {
                    AudioScriptableObjects.enable.Play(true);
                }
                else
                {
                    AudioScriptableObjects.disable.Play(true);
                }
            }
        }
    }
}