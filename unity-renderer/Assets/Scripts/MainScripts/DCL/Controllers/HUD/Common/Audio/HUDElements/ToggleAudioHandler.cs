using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleAudioHandler : MonoBehaviour, IPointerDownHandler
{
    Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnValueChanged);
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

    public void OnValueChanged(bool isOn)
    {
        if (toggle != null)
        {
            if (toggle.interactable)
            {
                if (isOn)
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