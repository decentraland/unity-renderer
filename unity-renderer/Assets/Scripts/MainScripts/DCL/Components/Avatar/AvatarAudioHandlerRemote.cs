using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;

public class AvatarAudioHandlerRemote : MonoBehaviour
{
    [SerializeField] private AudioContainer audioContainer;
    [SerializeField] private StickersController stickersController;
    
    const float WALK_INTERVAL_SEC = 0.37f, RUN_INTERVAL_SEC = 0.25f;
    float nextFootstepTime = 0f;

    AudioEvent footstepJump;
    AudioEvent footstepLand;
    AudioEvent footstepWalk;
    AudioEvent footstepRun;
    AudioEvent clothesRustleShort;

    GameObject rendererContainer;
    new Renderer renderer;

    AvatarAnimatorLegacy.BlackBoard blackBoard;
    bool isGroundedPrevious = true;

    public AvatarAnimatorLegacy avatarAnimatorLegacy;
    bool globalRendererIsReady;

    private Camera mainCamera;

    Transform footL;
    Transform footR;

    private void Start()
    {
        footstepJump = audioContainer.GetEvent("FootstepJump");
        footstepLand = audioContainer.GetEvent("FootstepLand");
        footstepWalk = audioContainer.GetEvent("FootstepWalk");
        footstepRun = audioContainer.GetEvent("FootstepRun");
        clothesRustleShort = audioContainer.GetEvent("ClothesRustleShort");

        // Lower volume of jump/land/clothes
        footstepJump.source.volume *= 0.5f;
        footstepLand.source.volume *= 0.5f;
        clothesRustleShort.source.volume *= 0.5f;

        if (avatarAnimatorLegacy != null)
        {
            blackBoard = avatarAnimatorLegacy.blackboard;
        }

        globalRendererIsReady = CommonScriptableObjects.rendererState.Get();
        CommonScriptableObjects.rendererState.OnChange += OnGlobalRendererStateChange;
    }

    void OnGlobalRendererStateChange(bool current, bool previous) { globalRendererIsReady = current; }

    public void Init(GameObject rendererContainer)
    {
        this.rendererContainer = rendererContainer;

        // Get references to body parts
        Transform[] children = rendererContainer.GetComponentsInChildren<Transform>();
        footL = AvatarBodyPartReferenceUtility.GetLeftToe(children);
        footR = AvatarBodyPartReferenceUtility.GetRightToe(children);
    }

    private void Update()
    {
        if (blackBoard == null || !globalRendererIsReady)
            return;

        // Jumped
        if (!blackBoard.isGrounded && isGroundedPrevious)
        {
            if (footstepJump != null)
                footstepJump.Play(true);
            if (stickersController != null && footR != null)
                stickersController.PlaySticker("footstepJump", footR.position, Vector3.up, false);
        }

        // Landed
        if (blackBoard.isGrounded && !isGroundedPrevious)
        {
            if (footstepLand != null)
                footstepLand.Play(true);

            if (stickersController != null && footL != null && footR != null)
            {
                stickersController.PlaySticker("footstepLand",
                    Vector3.Lerp(footL.position, footR.position, 0.5f),
                    Vector3.up,
                    false);
            }
        }

        // Simulate footsteps when avatar is not visible
        if (renderer != null)
        {
            SimulateFootsteps();
        }
        else
        {
            if (rendererContainer != null)
            {
                //NOTE(Mordi): The renderer takes a while to get ready, so we need to check it continually until it can be fetched
                renderer = rendererContainer.GetComponentInChildren<Renderer>();
            }
        }

        isGroundedPrevious = blackBoard.isGrounded;
    }

    bool AvatarIsInView()
    {
        if (renderer.isVisible)
            return true;

        if (Camera.main == null)
            return false;

        // NOTE(Mordi): In some cases, the renderer will report false even if the avatar is visible.
        // Therefore we must check whether or not the avatar is in the camera's view.

        if ( mainCamera == null )
            mainCamera = Camera.main;

        if (mainCamera == null)
            return false;

        Vector3 point = mainCamera.WorldToViewportPoint(transform.position);

        if (point.z > 0f)
        {
            if (point.x >= 0f && point.x <= 1f)
            {
                if (point.y >= 0f && point.y <= 1f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void SimulateFootsteps()
    {
        if (!AvatarIsInView() && (blackBoard.movementSpeed / Time.deltaTime) > 1f && blackBoard.isGrounded)
        {
            if (Time.time >= nextFootstepTime)
            {
                if ((blackBoard.movementSpeed / Time.deltaTime) > 6f)
                {
                    if (footstepRun != null)
                        footstepRun.Play(true);

                    if (clothesRustleShort != null)
                        clothesRustleShort.Play(true);

                    nextFootstepTime = Time.time + RUN_INTERVAL_SEC;
                }
                else
                {
                    if (footstepWalk != null)
                        footstepWalk.Play(true);

                    if (clothesRustleShort != null)
                        clothesRustleShort.PlayScheduled(Random.Range(0.05f, 0.1f));

                    nextFootstepTime = Time.time + WALK_INTERVAL_SEC;
                }
            }
        }
    }
}