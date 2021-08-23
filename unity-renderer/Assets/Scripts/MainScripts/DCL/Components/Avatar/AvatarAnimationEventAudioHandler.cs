using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimationEventAudioHandler : MonoBehaviour
{
    ParticleSystem footstepParticleSystem, footstepParticleSystemPrefab;

    Transform leftFoot, rightFoot;

    AudioEvent footstepLight;
    AudioEvent footstepSlide;
    AudioEvent footstepWalk;
    AudioEvent footstepRun;
    AudioEvent clothesRustleShort;
    AudioEvent clap;
    AudioEvent throwMoney;
    AudioEvent blowKiss;

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

        // Find feet
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name == "Avatar_LeftToeBase")
                leftFoot = children[i];
            if (children[i].name == "Avatar_RightToeBase")
                rightFoot = children[i];
        }

        // Load particle system
        footstepParticleSystemPrefab = Resources.Load<ParticleSystem>("AvatarFootstepParticleSystem");
        footstepParticleSystem = Instantiate(footstepParticleSystemPrefab, transform);
        footstepParticleSystem.Play();
    }

    public void AnimEvent_FootstepLight() { TryPlayingEvent(footstepLight); }

    public void AnimEvent_FootstepSlide() { TryPlayingEvent(footstepSlide); }

    public void AnimEvent_FootstepWalkLeft()
    {
        TryPlayingEvent(footstepWalk);
        SpawnFootstepParticles(leftFoot, 1);
    }

    public void AnimEvent_FootstepWalkRight() {
        TryPlayingEvent(footstepWalk);
        SpawnFootstepParticles(rightFoot, 1);
    }

    public void AnimEvent_FootstepRunLeft()
    {
        TryPlayingEvent(footstepRun);
        SpawnFootstepParticles(leftFoot, 3);
    }

    public void AnimEvent_FootstepRunRight() {
        TryPlayingEvent(footstepRun);
        SpawnFootstepParticles(rightFoot, 3);
    }

    public void AnimEvent_ClothesRustleShort() { TryPlayingEvent(clothesRustleShort); }

    public void AnimEvent_Clap() { TryPlayingEvent(clap); }

    public void AnimEvent_ThrowMoney() { TryPlayingEvent(throwMoney); }

    public void AnimEvent_BlowKiss() { TryPlayingEvent(blowKiss); }

    void TryPlayingEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }

    void SpawnFootstepParticles(Transform foot, int amount) {
        footstepParticleSystem.transform.position = foot.position;
        footstepParticleSystem.Emit(amount);
    }
}