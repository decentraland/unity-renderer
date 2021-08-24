using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

public class AvatarAnimationEventHandler : MonoBehaviour
{
    Transform leftFoot, rightFoot, leftHand;

    AudioEvent footstepLight;
    AudioEvent footstepSlide;
    AudioEvent footstepWalk;
    AudioEvent footstepRun;
    AudioEvent clothesRustleShort;
    AudioEvent clap;
    AudioEvent throwMoney;
    AudioEvent blowKiss;

    AvatarParticleSystemsHandler particleSystemsHandler;

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

        // Find body parts
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name == "Avatar_LeftToeBase")
                leftFoot = children[i];
            if (children[i].name == "Avatar_RightToeBase")
                rightFoot = children[i];
            if (children[i].name == "Avatar_LeftHand")
                leftHand = children[i];
        }

        particleSystemsHandler = FindObjectOfType<AvatarParticleSystemsHandler>();
    }

    public void AnimEvent_FootstepLight() { TryPlayingEvent(footstepLight); }

    public void AnimEvent_FootstepSlide() { TryPlayingEvent(footstepSlide); }

    public void AnimEvent_FootstepWalkLeft() { TryPlayingEvent(footstepWalk); }

    public void AnimEvent_FootstepWalkRight() { TryPlayingEvent(footstepWalk); }

    public void AnimEvent_FootstepRunLeft()
    {
        TryPlayingEvent(footstepRun);
        particleSystemsHandler.EmitFootstepParticles(leftFoot.position, 3);
    }

    public void AnimEvent_FootstepRunRight() {
        TryPlayingEvent(footstepRun);
        particleSystemsHandler.EmitFootstepParticles(rightFoot.position, 3);
    }

    public void AnimEvent_ClothesRustleShort() { TryPlayingEvent(clothesRustleShort); }

    public void AnimEvent_Clap()
    {
        TryPlayingEvent(clap);
    }

    public void AnimEvent_ThrowMoney()
    {
        TryPlayingEvent(throwMoney);
        particleSystemsHandler.EmitMoneyParticles(leftHand.position, 1);
    }

    public void AnimEvent_BlowKiss()
    {
        TryPlayingEvent(blowKiss);
        particleSystemsHandler.EmitHeartParticles(leftHand.position, 1);
    }

    void TryPlayingEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }
}