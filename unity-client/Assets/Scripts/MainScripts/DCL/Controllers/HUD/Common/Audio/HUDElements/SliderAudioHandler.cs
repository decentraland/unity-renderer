using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderAudioHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    const float INCREMENT_SOUND_INTERVAL = 0.02f;

    HUDAudioPlayer audioPlayer;
    Slider slider;
    float valueIncrementTimer = 0f;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void Update()
    {
        if (valueIncrementTimer > 0f)
        {
            valueIncrementTimer -= Time.deltaTime;
            if (valueIncrementTimer < 0f)
            {
                valueIncrementTimer = 0f;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            slider != null &&
            slider.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonClick);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (audioPlayer != null &&
            slider != null &&
            slider.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonRelease);
        }
    }

    void OnValueChanged(float value)
    {
        if (audioPlayer != null && valueIncrementTimer == 0f)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.valueChange, 1f + ((slider.value - slider.minValue) / (slider.maxValue - slider.minValue)) * 1.5f);
            valueIncrementTimer = INCREMENT_SOUND_INTERVAL;
        }
    }
}
