using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleHandleAudioHandler : MonoBehaviour, IPointerEnterHandler
{
    HUDAudioPlayer audioPlayer;
    [SerializeField]
    Toggle toggle;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            toggle != null &&
            toggle.interactable &&
            !Input.GetMouseButton(0))
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonHover);
        }
    }
}