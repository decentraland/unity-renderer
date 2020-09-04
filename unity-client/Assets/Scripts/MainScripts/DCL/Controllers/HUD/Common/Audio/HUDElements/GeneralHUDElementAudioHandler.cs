using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralHUDElementAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    HUDAudioPlayer audioPlayer;
    [SerializeField]
    HUDAudioPlayer.Sound hoverSound = HUDAudioPlayer.Sound.buttonHover;
    [SerializeField]
    bool playHover = false, playClick = true, playRelease = true;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHover && audioPlayer != null)
            audioPlayer.Play(hoverSound);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playClick && audioPlayer != null)
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonClick);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (playRelease && audioPlayer != null)
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonRelease);
    }
}