using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

public class AvatarAnimationEventHandler : MonoBehaviour
{
    const string ANIM_NAME_KISS = "kiss", ANIM_NAME_MONEY = "money", ANIM_NAME_CLAP = "clap";
    const float MIN_EVENT_WAIT_TIME = 0.1f;

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

    Animation anim;

    float lastEventTime;

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

        anim = GetComponent<Animation>();
    }

    public void AnimEvent_FootstepLight() { TryPlayingEvent(footstepLight); }

    public void AnimEvent_FootstepSlide() { TryPlayingEvent(footstepSlide); }

    public void AnimEvent_FootstepWalkLeft() { TryPlayingEvent(footstepWalk); }

    public void AnimEvent_FootstepWalkRight() { TryPlayingEvent(footstepWalk); }

    public void AnimEvent_FootstepRunLeft()
    {
        TryPlayingEvent(footstepRun);
        particleSystemsHandler.EmitFootstepParticles(bodyPartReferenceHandler.footL.position, Vector3.up, 3);
    }

    public void AnimEvent_FootstepRunRight() {
        TryPlayingEvent(footstepRun);
        particleSystemsHandler.EmitFootstepParticles(bodyPartReferenceHandler.footR.position, Vector3.up, 3);
    }

    public void AnimEvent_ClothesRustleShort() { TryPlayingEvent(clothesRustleShort); }

    public void AnimEvent_Clap()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_CLAP))
            return;

        TryPlayingEvent(clap);
        particleSystemsHandler.EmitClapParticles(bodyPartReferenceHandler.handR.position, Vector3.up, 1);
        UpdateEventTime();
    }

    public void AnimEvent_ThrowMoney()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_MONEY))
            return;

        TryPlayingEvent(throwMoney);
        particleSystemsHandler.EmitMoneyParticles(bodyPartReferenceHandler.handL.position, bodyPartReferenceHandler.handL.rotation.eulerAngles, 1);
        UpdateEventTime();
    }

    public void AnimEvent_BlowKiss()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_KISS))
            return;

        TryPlayingEvent(blowKiss);
        StartCoroutine(EmitHeartParticle());
        UpdateEventTime();
    }

    IEnumerator EmitHeartParticle()
    {
        yield return new WaitForSeconds(0.8f);

        particleSystemsHandler.EmitHeartParticles(bodyPartReferenceHandler.handR.position, transform.rotation.eulerAngles, 1);
    }

    void TryPlayingEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }

    bool AnimationWeightIsOverThreshold(float threshold, string animationName)
    {
        if (anim != null) {
            if (anim.isPlaying) {
                foreach (AnimationState state in anim) {
                    if (state.name == animationName) {
                        if (state.weight > threshold)
                            return true;
                        break;
                    }
                }
            }
        }

        return false;
    }

    void UpdateEventTime()
    {
        lastEventTime = Time.realtimeSinceStartup;
    }

    bool LastEventWasTooRecent()
    {
        return lastEventTime + MIN_EVENT_WAIT_TIME >= Time.realtimeSinceStartup;
    }
}