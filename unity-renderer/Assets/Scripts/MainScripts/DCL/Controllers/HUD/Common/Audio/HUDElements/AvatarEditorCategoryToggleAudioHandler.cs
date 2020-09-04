using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvatarEditorCategoryToggleAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    HUDAudioPlayer audioPlayer;
    Selectable selectable;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
        selectable = GetComponent<Selectable>();
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
            audioPlayer.ResetListItemAppearPitch();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            selectable != null &&
            selectable.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonRelease);
        }
    }
}
