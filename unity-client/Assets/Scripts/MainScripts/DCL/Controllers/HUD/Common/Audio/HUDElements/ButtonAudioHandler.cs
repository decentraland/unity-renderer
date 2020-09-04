using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    HUDAudioPlayer audioPlayer;
    Selectable selectable;
    [SerializeField]
    HUDAudioPlayer.Sound extraClickSound = HUDAudioPlayer.Sound.none;
    [SerializeField]
    bool playHoverSound = true;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHoverSound)
            return;

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

    public void OnPointerUp(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            selectable != null &&
            selectable.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonRelease);

            if (extraClickSound != HUDAudioPlayer.Sound.none)
            {
                audioPlayer.Play(extraClickSound);
            }
        }
    }
}