using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAudioHandlerLocal : MonoBehaviour
{
    const float WALK_RUN_CROSSOVER_DISTANCE = 7.3f;
    const float WALK_INTERVAL_SEC = 0.4f;
    const float RUN_INTERVAL_SEC = 0.27f;

    float intervalTimer = 0f;

    AudioEvent footstepJump;
    AudioEvent footstepLand;
    AudioEvent footstepWalk;
    AudioEvent footstepRun;
    AudioEvent clothesRustleShort;

    CameraController cameraController;

    private void Start()
    {
        DCLCharacterController dclCharacterController = transform.parent.GetComponent<DCLCharacterController>();
        if (dclCharacterController != null)
        {
            dclCharacterController.OnJump += OnJump;
            dclCharacterController.OnHitGround += OnLand;
            dclCharacterController.OnMoved += OnWalk;
        }

        AudioContainer ac = GetComponent<AudioContainer>();
        footstepJump = ac.GetEvent("FootstepJump");
        footstepLand = ac.GetEvent("FootstepLand");
        footstepWalk = ac.GetEvent("FootstepWalk");
        footstepRun = ac.GetEvent("FootstepRun");
        clothesRustleShort = ac.GetEvent("ClothesRustleShort");
    }

    void OnJump()
    {
        if (footstepJump != null)
            footstepJump.Play(true);
    }

    void OnLand()
    {
        if (footstepLand != null)
            footstepLand.Play(true);
    }

    // Faking footsteps when in first-person mode, since animations won't play
    void OnWalk(float distance)
    {
        if (cameraController == null)
        {
            GameObject camObj = GameObject.Find("CameraController");
            if (camObj != null)
                cameraController =  camObj.GetComponent<CameraController>();
            return;
        }

        if (cameraController.currentCameraState.name != "FirstPerson")
            return;

        if (intervalTimer < 0f)
        {
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
