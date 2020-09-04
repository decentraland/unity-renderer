using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleAudioHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    HUDAudioPlayer audioPlayer;
    Toggle toggle;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
        toggle = GetComponent<Toggle>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            toggle != null &&
            toggle.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonClick);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            toggle != null &&
            toggle.interactable)
        {
            if (toggle.isOn)
            {
                audioPlayer.Play(HUDAudioPlayer.Sound.disable);
            }
            else
            {
                audioPlayer.Play(HUDAudioPlayer.Sound.enable);
            }
        }
    }
}