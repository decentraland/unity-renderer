using UnityEngine;

namespace DCL
{
    public class StickerAnimationListener : MonoBehaviour
    {
        private const float WEIGHT_THRESHOLD = 0.5f;
        private StickersController stickersController;

        private void Awake() { stickersController = GetComponentInParent<StickersController>(); }

        //It's going to be called through an AnimationEvent
        private void PlaySticker(AnimationEvent animEvent)
        {
            if (string.IsNullOrEmpty(animEvent.stringParameter))
                return;

            if (animEvent.animationState.weight < WEIGHT_THRESHOLD)
                return;

            stickersController?.PlaySticker(animEvent.stringParameter);
        }
    }
}