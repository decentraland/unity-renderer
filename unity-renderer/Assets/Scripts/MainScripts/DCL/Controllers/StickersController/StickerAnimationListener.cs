using UnityEngine;

public class StickerAnimationListener : MonoBehaviour
{
    private StickersController stickersController;

    private void Awake() { stickersController = GetComponentInParent<StickersController>(); }

    //It's going to be called through an AnimationEvent
    private void PlaySticker(AnimationEvent animEvent)
    {
        if (string.IsNullOrEmpty(animEvent.stringParameter))
            return;

        if (animEvent.animationState.weight < 0.5f)
            return;

        stickersController?.PlayEmote(animEvent.stringParameter);
    }
}