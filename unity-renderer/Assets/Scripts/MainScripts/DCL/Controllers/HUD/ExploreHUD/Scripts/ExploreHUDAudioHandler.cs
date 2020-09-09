using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreHUDAudioHandler : MonoBehaviour
{
    [SerializeField]
    GotoMagicButton magicButton;

    [SerializeField]
    AudioEvent eventMagicPointerEnter, eventMagicPointerExit, eventMagicButtonPressed;

    float magicPointerEnterLastPlayed = 0f;

    private void Start()
    {
        magicButton.OnGotoMagicPointerEnter += OnMagicButtonEnter;
        magicButton.onGotoMagicPointerExit += OnMagicButtonExit;
        magicButton.OnGotoMagicPressed += OnMagicButtonPressed;
    }

    void OnMagicButtonEnter()
    {
        magicPointerEnterLastPlayed = Time.fixedTime;
        eventMagicPointerEnter.Play(true);
    }

    void OnMagicButtonExit()
    {
        if (magicPointerEnterLastPlayed < Time.fixedTime -0.3f)
            eventMagicPointerExit.Play(true);
    }

    void OnMagicButtonPressed()
    {
        eventMagicButtonPressed.Play(true);
    }
}
