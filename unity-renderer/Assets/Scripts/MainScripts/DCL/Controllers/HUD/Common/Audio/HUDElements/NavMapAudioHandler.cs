using UnityEngine;
using UnityEngine.EventSystems;

public class NavMapAudioHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioScriptableObjects.buttonClick.Play(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AudioScriptableObjects.buttonRelease.Play(true);
    }
}