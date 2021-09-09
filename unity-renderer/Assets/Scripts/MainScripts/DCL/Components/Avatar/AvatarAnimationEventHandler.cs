using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

public class AvatarAnimationEventHandler : MonoBehaviour
{
    AudioEvent footstepLight;
    AudioEvent footstepSlide;
    AudioEvent footstepWalk;
    AudioEvent footstepRun;
    AudioEvent clothesRustleShort;
    AudioEvent clap;
    AudioEvent throwMoney;
    AudioEvent blowKiss;

    AvatarParticleSystemsHandler particleSystemsHandler;
    AvatarBodyPartReferenceHandler bodyPartReferenceHandler;

    Animation debug;

    public void Init(AudioContainer audioContainer)
    {
        if (audioContainer == null)
            return;

        footstepLight = audioContainer.GetEvent("FootstepLight");
        footstepSlide = audioContainer.GetEvent("FootstepSlide");
        footstepWalk = audioContainer.GetEvent("FootstepWalk");
        footstepRun = audioContainer.GetEvent("FootstepRun");
        clothesRustleShort = audioContainer.GetEvent("ClothesRustleShort");
        clap = audioContainer.GetEvent("ExpressionClap");
        throwMoney = audioContainer.GetEvent("ExpressionThrowMoney");
        blowKiss = audioContainer.GetEvent("ExpressionBlowKiss");

        particleSystemsHandler = FindObjectOfType<AvatarParticleSystemsHandler>();
        bodyPartReferenceHandler = GetComponent<AvatarBodyPartReferenceHandler>();

        debug = GetComponent<Animation>();
    }

    private void Update() {
        Debug.Log($"IsPlaying = {debug.isPlaying}");
    }

    public void AnimEvent_FootstepLight() { TryPlayingEvent(footstepLight); }

    public void AnimEvent_FootstepSlide() { TryPlayingEvent(footstepSlide); }

    public void AnimEvent_FootstepWalkLeft() { TryPlayingEvent(footstepWalk); }

    public void AnimEvent_FootstepWalkRight() { TryPlayingEvent(footstepWalk); }

    public void AnimEvent_FootstepRunLeft()
    {
        TryPlayingEvent(footstepRun);
        particleSystemsHandler.EmitFootstepParticles(bodyPartReferenceHandler.footL.position, 3);
    }

    public void AnimEvent_FootstepRunRight() {
        TryPlayingEvent(footstepRun);
        particleSystemsHandler.EmitFootstepParticles(bodyPartReferenceHandler.footR.position, 3);
    }

    public void AnimEvent_ClothesRustleShort() { TryPlayingEvent(clothesRustleShort); }

    public void AnimEvent_Clap()
    {
        TryPlayingEvent(clap);
    }

    public void AnimEvent_ThrowMoney()
    {
        TryPlayingEvent(throwMoney);
        particleSystemsHandler.EmitMoneyParticles(bodyPartReferenceHandler.handL.position, 1);
        Debug.Log("Moeny!");
    }

    public void AnimEvent_BlowKiss()
    {
        TryPlayingEvent(blowKiss);
        particleSystemsHandler.EmitHeartParticles(bodyPartReferenceHandler.handR.position, 1);
        Debug.Log("Kiss!");
    }

    void TryPlayingEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }
}