using UnityEngine;

public class NavMapAudioHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (HUDAudioPlayer.i != null)
            {
                HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.buttonClick);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (HUDAudioPlayer.i != null)
            {
                HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.buttonRelease);
            }
        }
    }
}
