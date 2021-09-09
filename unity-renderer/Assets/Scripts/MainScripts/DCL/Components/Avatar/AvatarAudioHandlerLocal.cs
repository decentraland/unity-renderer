using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAudioHandlerLocal : MonoBehaviour
{
    const float WALK_RUN_CROSSOVER_DISTANCE = 7.3f;
    const float WALK_INTERVAL_SEC = 0.4f;
    const float RUN_INTERVAL_SEC = 0.27f;

    [SerializeField]
    AvatarParticleSystemsHandler particleSystemsHandler;
    [SerializeField]
    Vector3 jumpLandParticlesOffset;

    float intervalTimer = 0f;

    AudioEvent footstepJump;
    AudioEvent footstepLand;
    AudioEvent footstepWalk;
    AudioEvent footstepRun;
    AudioEvent clothesRustleShort;

    private void Start()
    {
        var characterController = DCLCharacterController.i;

        if (characterController != null)
        {
            characterController.OnJump += OnJump;
            characterController.OnHitGround += OnLand;
            characterController.OnMoved += OnWalk;
        }

        AudioContainer ac = GetComponent<AudioContainer>();
        footstepJump = ac.GetEvent("FootstepJump");
        footstepLand = ac.GetEvent("FootstepLand");
        footstepWalk = ac.GetEvent("FootstepWalk");
        footstepRun = ac.GetEvent("FootstepRun");
        clothesRustleShort = ac.GetEvent("ClothesRustleShort");

        particleSystemsHandler = FindObjectOfType<AvatarParticleSystemsHandler>();
    }

    void OnJump()
    {
        if (footstepJump != null)
            footstepJump.Play(true);
        if (particleSystemsHandler != null)
            particleSystemsHandler.EmitFootstepParticles(transform.position + jumpLandParticlesOffset, Vector3.up, 5);
    }

    void OnLand()
    {
        if (footstepLand != null)
            footstepLand.Play(true);
        if (particleSystemsHandler != null)
            particleSystemsHandler.EmitFootstepParticles(transform.position + jumpLandParticlesOffset, Vector3.up, 10);
    }

    // Faking footsteps when in first-person mode, since animations won't play
    void OnWalk(float distance)
    {
        if (CommonScriptableObjects.cameraMode.Get() != CameraMode.ModeId.FirstPerson)
            return;

        if (intervalTimer < 0f)
        {
            distance /= Time.deltaTime;

            if (distance > WALK_RUN_CROSSOVER_DISTANCE)
            {
                if (footstepRun != null)
                    footstepRun.Play(true);

                if (clothesRustleShort != null)
                    clothesRustleShort.Play(true);

                intervalTimer = RUN_INTERVAL_SEC;
            }
            else
            {
                if (footstepWalk != null)
                    footstepWalk.Play(true);

                if (clothesRustleShort != null)
                    clothesRustleShort.PlayScheduled(Random.Range(0.05f, 0.1f));

                intervalTimer = WALK_INTERVAL_SEC;
            }
        }

        intervalTimer -= Time.deltaTime;
    }
}