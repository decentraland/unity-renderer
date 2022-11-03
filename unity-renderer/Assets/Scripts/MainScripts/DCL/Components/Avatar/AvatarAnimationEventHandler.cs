using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;

public class AvatarAnimationEventHandler : MonoBehaviour
{
    const string ANIM_NAME_KISS = "kiss", ANIM_NAME_MONEY = "money", ANIM_NAME_CLAP = "clap", ANIM_NAME_SNOWFLAKE = "snowfall", ANIM_NAME_HOHOHO = "hohoho";
    const float MIN_EVENT_WAIT_TIME = 0.1f;
    const float AVATAR_OFFSET = 0.755f; //USE IT WHEN DEALING WITH LOCAL SPACE

    AudioEvent footstepLight;
    AudioEvent footstepSlide;
    AudioEvent footstepWalk;
    AudioEvent footstepRun;
    AudioEvent clothesRustleShort;
    AudioEvent clap;
    AudioEvent throwMoney;
    AudioEvent blowKiss;

    Animation anim;

    float lastEventTime;

    StickersController stickersController;

    Transform footL;
    Transform footR;
    Transform handL;
    Transform handR;

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

        anim = GetComponent<Animation>();
        stickersController = GetComponentInParent<StickersController>();

        Transform[] children = GetComponentsInChildren<Transform>();
        footL = AvatarBodyPartReferenceUtility.GetLeftToe(children);
        footR = AvatarBodyPartReferenceUtility.GetRightToe(children);
        handL = AvatarBodyPartReferenceUtility.GetLeftHand(children);
        handR = AvatarBodyPartReferenceUtility.GetRightHand(children);
    }

    public void AnimEvent_FootstepLight() { PlayAudioEvent(footstepLight); }

    public void AnimEvent_FootstepSlide() { PlayAudioEvent(footstepSlide); }

    public void AnimEvent_FootstepWalkLeft() { PlayAudioEvent(footstepWalk); }

    public void AnimEvent_FootstepWalkRight() { PlayAudioEvent(footstepWalk); }

    public void AnimEvent_FootstepRunLeft()
    {
        PlayAudioEvent(footstepRun);
        PlaySticker("footstep", footL.position, Vector3.up);
    }

    public void AnimEvent_FootstepRunRight()
    {
        PlayAudioEvent(footstepRun);
        PlaySticker("footstep", footR.position, Vector3.up);
    }

    public void AnimEvent_ClothesRustleShort() { PlayAudioEvent(clothesRustleShort); }

    public void AnimEvent_Clap()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_CLAP))
            return;

        PlayAudioEvent(clap);
        PlaySticker("clap", handR.position - transform.position + (Vector3.up * AVATAR_OFFSET), Vector3.up, true);
        UpdateEventTime();
    }

    public void AnimEvent_ThrowMoney()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_MONEY))
            return;

        PlayAudioEvent(throwMoney);
        PlaySticker("money", handL.position - transform.position + (Vector3.up * AVATAR_OFFSET), handL.rotation.eulerAngles, true);
        UpdateEventTime();
    }

    public void AnimEvent_BlowKiss()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_KISS))
            return;

        PlayAudioEvent(blowKiss);
        StartCoroutine(EmitHeartParticle());
        UpdateEventTime();
    }

    IEnumerator EmitHeartParticle()
    {
        yield return new WaitForSeconds(0.8f);
        PlaySticker("heart", handR.position - transform.position + (Vector3.up * AVATAR_OFFSET), transform.rotation.eulerAngles, true);
    }

    public void AnimEvent_Snowflakes()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_SNOWFLAKE))
            return;

        PlaySticker("snowflakes", Vector3.up * AVATAR_OFFSET, Vector3.zero, true);
    }

    public void AnimEvent_Hohoho()
    {
        if (LastEventWasTooRecent())
            return;
        
        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_HOHOHO))
            return;

        PlaySticker("hohoho", Vector3.up * (1.5f + AVATAR_OFFSET), Vector3.zero, true);
    }

    void PlayAudioEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }

    bool AnimationWeightIsOverThreshold(float threshold, string animationName)
    {
        if (anim != null)
        {
            if (anim.isPlaying)
            {
                foreach (AnimationState state in anim)
                {
                    if (state.name == animationName)
                    {
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

    /// <summary>
    /// Plays a sticker.
    /// </summary>
    /// <param name="id">ID string of sticker</param>
    /// <param name="position">Position in world space</param>
    /// <param name="rotation">Euler angles</param>
    void PlaySticker(string id, Vector3 position, Vector3 direction, bool followTransform = false)
    {
        if (stickersController != null)
            stickersController.PlaySticker(id, position, direction, followTransform);
    }
}