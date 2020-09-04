using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollbarHandleAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    HUDAudioPlayer audioPlayer;
    [SerializeField]
    Selectable selectable;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            selectable != null &&
            selectable.interactable &&
            !Input.GetMouseButton(0))
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonHover);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            selectable != null &&
            selectable.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonClick);
        }
    }
}