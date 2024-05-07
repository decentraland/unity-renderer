using System.Collections;
using UnityEngine;
using DCL;

public class AvatarAnimationEventHandler : MonoBehaviour
{
    private const string FOOTSTEP_NAME = "footstep", HEART_NAME = "heart";
    private const string ANIM_NAME_KISS = "kiss", ANIM_NAME_MONEY = "money", ANIM_NAME_CLAP = "clap", ANIM_NAME_SNOWFLAKE = "snowfall", ANIM_NAME_HOHOHO = "hohoho";
    private const float MIN_EVENT_WAIT_TIME = 0.1f;
    private const float HOHOHO_OFFSET = 1.5f;

    private AudioEvent footstepLight;
    private AudioEvent footstepSlide;
    private AudioEvent footstepWalk;
    private AudioEvent footstepRun;
    private AudioEvent clothesRustleShort;
    private AudioEvent clap;
    private AudioEvent throwMoney;
    private AudioEvent blowKiss;

    private Animation anim;

    private float lastEventTime;

    private StickersController stickersController;

    private int renderingLayer;

    private Transform footL;
    private Transform footR;
    private Transform handL;
    private Transform handR;

    public void Init(AudioContainer audioContainer, int renderingLayer)
    {
        if (audioContainer == null)
            return;

        this.renderingLayer = renderingLayer;

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
        PlaySticker(FOOTSTEP_NAME, footL.position, Vector3.up, renderingLayer: renderingLayer);
    }

    public void AnimEvent_FootstepRunRight()
    {
        PlayAudioEvent(footstepRun);
        PlaySticker(FOOTSTEP_NAME, footR.position, Vector3.up, renderingLayer: renderingLayer);
    }

    public void AnimEvent_ClothesRustleShort() { PlayAudioEvent(clothesRustleShort); }

    public void AnimEvent_Clap()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_CLAP))
            return;

        PlayAudioEvent(clap);
        PlaySticker(ANIM_NAME_CLAP, handR.position, Vector3.up, true, renderingLayer: renderingLayer);
        UpdateEventTime();
    }

    public void AnimEvent_ThrowMoney()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_MONEY))
            return;

        PlayAudioEvent(throwMoney);
        PlaySticker(ANIM_NAME_MONEY, handL.position, handL.rotation.eulerAngles, true, renderingLayer: renderingLayer);
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

    private IEnumerator EmitHeartParticle()
    {
        yield return new WaitForSeconds(0.8f);
        PlaySticker(HEART_NAME, handR.position, transform.rotation.eulerAngles, true, renderingLayer: renderingLayer);
    }

    public void AnimEvent_Snowflakes()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_SNOWFLAKE))
            return;

        PlaySticker("snowflakes", transform.position, Vector3.zero, true, renderingLayer: renderingLayer);
    }

    public void AnimEvent_Hohoho()
    {
        if (LastEventWasTooRecent())
            return;

        if (!AnimationWeightIsOverThreshold(0.2f, ANIM_NAME_HOHOHO))
            return;

        PlaySticker(ANIM_NAME_HOHOHO, transform.position + Vector3.up * HOHOHO_OFFSET, Vector3.zero, true, renderingLayer: renderingLayer);
    }

    private void PlayAudioEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }

    private bool AnimationWeightIsOverThreshold(float threshold, string animationName)
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

    private void UpdateEventTime()
    {
        lastEventTime = Time.realtimeSinceStartup;
    }

    private bool LastEventWasTooRecent() =>
        lastEventTime + MIN_EVENT_WAIT_TIME >= Time.realtimeSinceStartup;

    /// <summary>
    /// Plays a sticker.
    /// </summary>
    /// <param name="id">ID string of sticker</param>
    /// <param name="position">Position in world space</param>
    /// <param name="rotation">Euler angles</param>
    private void PlaySticker(string id, Vector3 position, Vector3 direction, bool followTransform = false, int renderingLayer = 0)
    {
        if (stickersController != null)
            stickersController.PlaySticker(id, position, direction, followTransform, renderingLayer);
    }
}
