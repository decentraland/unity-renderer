using UnityEngine;

public class StickerAnimationListener : MonoBehaviour
{
    private StickersController stickersController;

    private void Awake() { stickersController = GetComponentInParent<StickersController>(); }

    //It's going to be called through an AnimationEvent
    private void PlaySticker(string id) { stickersController?.PlayEmote(id); }
}