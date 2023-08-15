using DCL;
using DCL.CameraTool;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using MainScripts.DCL.Controllers.CharacterController;
using System;
using UnityEngine;
using Environment = DCL.Environment;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterView : MonoBehaviour, ICharacterView
    {
        [SerializeField] private UnityEngine.CharacterController characterController;
        [SerializeField] private CharacterAnimationController animationController;

        [Header("Data")]
        [SerializeField] private CharacterControllerData data;
        [SerializeField] private GameObject shadowBlob;

        [Header("InputActions")]
        [SerializeField] private InputAction_Hold jumpAction;
        [SerializeField] private InputAction_Hold sprintAction;
        [SerializeField] private InputAction_Hold walkAction;
        [SerializeField] private InputAction_Measurable characterYAxis;
        [SerializeField] private InputAction_Measurable characterXAxis;

        [SerializeField] private Vector3Variable cameraForward;
        [SerializeField] private Vector3Variable cameraRight;

        [Header("Camera")]
        public CameraMode CameraMode;

        [Header("Old References")]
        public GameObject avatarGameObject;
        public GameObject firstPersonCameraGameObject;

        private DCLCharacterControllerV2 controller;
        private readonly DataStore_Player dataStorePlayer = DataStore.i.player;
        private bool initialPositionAlreadySet;
        private Vector3 lastPosition;
        private float originalFOV;

        private CharacterState characterState;
        private bool isRagdoll;

        private void Awake()
        {
            controller = new DCLCharacterControllerV2(this, data, jumpAction, sprintAction, walkAction, characterXAxis, characterYAxis, cameraForward, cameraRight, CameraMode, shadowBlob);

            characterState = controller.GetCharacterState();
            animationController.SetupCharacterState(characterState);

            // TODO: Remove this?
            CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);
            dataStorePlayer.playerWorldPosition.Set(Vector3.zero);
            CommonScriptableObjects.playerCoords.Set(Vector2Int.zero);
            dataStorePlayer.playerGridPosition.Set(Vector2Int.zero);
            CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.zero);
            CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
            var worldData = DataStore.i.Get<DataStore_World>();
            worldData.avatarTransform.Set(avatarGameObject.transform);
            worldData.fpsTransform.Set(firstPersonCameraGameObject.transform);
            CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            OnRenderingStateChanged(CommonScriptableObjects.rendererState.Get(), false);

            dataStorePlayer.lastTeleportPosition.OnChange += OnTeleport;
        }

        public void PostStart(FollowWithDamping cameraFollow)
        {
            cameraFollow.additionalHeight = characterController.height * 0.5f;
            animationController.PostStart(cameraFollow);
        }

        void OnRenderingStateChanged(bool isEnable, bool prevState)
        {
            this.enabled = isEnable && !DataStore.i.common.isSignUpFlow.Get();
        }

        [Obsolete]
        private void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            transform.position = CharacterGlobals.characterPosition.unityPosition;
        }

        // sent by kernel
        [UsedImplicitly]
        public void Teleport(string teleportPayload)
        {
            var tpsCamera = DataStore.i.camera.tpsCamera.Get();
            originalFOV = tpsCamera.m_Lens.FieldOfView;

            var newPosition = Utils.FromJsonWithNulls<Vector3>(teleportPayload);
            dataStorePlayer.lastTeleportPosition.Set(newPosition, notifyEvent: true);

            if (!initialPositionAlreadySet) { initialPositionAlreadySet = true; }
        }

        private void OnTeleport(Vector3 current, Vector3 previous)
        {
            ReportPosition(current);
        }

        private void ReportPosition(Vector3 newPosition)
        {
            lastPosition = CharacterGlobals.characterPosition.worldPosition;
            CharacterGlobals.characterPosition.worldPosition = newPosition;
            transform.position = CharacterGlobals.characterPosition.unityPosition;
            Environment.i.platform.physicsSyncController?.MarkDirty();
            CommonScriptableObjects.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);
            dataStorePlayer.playerWorldPosition.Set(CharacterGlobals.characterPosition.worldPosition);
            Vector2Int playerPosition = Utils.WorldToGridPosition(CharacterGlobals.characterPosition.worldPosition);
            CommonScriptableObjects.playerCoords.Set(playerPosition);
            dataStorePlayer.playerGridPosition.Set(playerPosition);
            dataStorePlayer.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);

            if (ShouldReportMovement(lastPosition))
            {
                if (ShouldReportMovement(lastPosition, useThreshold: true))
                    ReportMovement();

                lastPosition = transform.position;
            }
        }

        private static bool ShouldReportMovement(Vector3 previousPosition, bool useThreshold = false)
        {
            if (useThreshold)
                return Vector3.Distance(CharacterGlobals.characterPosition.worldPosition, previousPosition) > 0.001f;
            else
                return CharacterGlobals.characterPosition.worldPosition != previousPosition;
        }

        private void ReportMovement()
        {
            var reportPosition = CharacterGlobals.characterPosition.worldPosition;
            var compositeRotation = Quaternion.LookRotation(transform.forward);
            var cameraRotation = Quaternion.LookRotation(cameraForward.Get());

            if (initialPositionAlreadySet)
                WebInterface.ReportPosition(reportPosition, compositeRotation, characterController.height, cameraRotation);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (!isRagdoll)
                    Ragdollize();
                else
                    DeRagdollize();
            }

            if (isRagdoll) return;

            // remove this after we are done with debugging
            characterController.radius = data.characterControllerRadius;
            characterController.skinWidth = characterController.radius * 0.1f; // its recommended that its 10% of the radius

            controller.Update(Time.deltaTime);

            // FOV HACK, move this elsewhere, this increases the FOV when running
            var tpsCamera = DataStore.i.camera.tpsCamera.Get();
            bool isFovHigher = characterState.SpeedState == SpeedState.RUN && Flat(characterState.TotalVelocity).magnitude >= characterState.MaxVelocity * 0.35f;
            float targetFov = isFovHigher ? originalFOV + 15 : originalFOV;
            float fovSpeed = isFovHigher ? 20 : 50;
            tpsCamera.m_Lens.FieldOfView = Mathf.MoveTowards(tpsCamera.m_Lens.FieldOfView, targetFov, fovSpeed * Time.deltaTime);
        }

        // The implementation is super hacky, we need to find a cleaner way of doing this if we are ever using this feature
        private void Ragdollize()
        {
            Physics.autoSimulation = true;
            var cameraTarget = GameObject.Find("CharacterCameraTarget ");
            var damp = cameraTarget.GetComponent<FollowWithDamping>();
            // let the camera follow the avatar
            damp.target = RecursiveFindChild(transform, "Avatar_Neck");

            isRagdoll = true;
            GetComponentInChildren<Animator>().enabled = false;
            characterController.enabled = false;
            GetComponentInChildren<RagdollController>().Ragdollize();
            damp.additionalHeight = 0;
        }

        private void DeRagdollize()
        {
            Animator animator = GetComponentInChildren<Animator>();
            Physics.autoSimulation = false;
            var cameraTarget = GameObject.Find("CharacterCameraTarget ");
            var damp = cameraTarget.GetComponent<FollowWithDamping>();
            damp.target = animator.transform;

            isRagdoll = false;
            GetComponentInChildren<RagdollController>().DeRagdollize();
            animator.enabled = true;
            characterController.enabled = true;
            damp.additionalHeight = 0.835f;
        }

        private Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if(child.name == childName)
                {
                    return child;
                }

                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                    return found;
            }
            return null;
        }
        private Vector3 Flat(Vector3 vector3)
        {
            vector3.y = 0;
            return vector3;
        }

        public (bool isGrounded, Vector3 deltaPosition, bool wallHit) Move(Vector3 delta)
        {
            if (!initialPositionAlreadySet) return (true, transform.position, false);

            var previousPosition = transform.position;
            var collisionFlags = characterController.Move(delta);
            Vector3 currentPosition = transform.position;
            var deltaPosition = previousPosition - currentPosition;

            if (currentPosition.y < 0) { transform.position = new Vector3(currentPosition.x, 0, currentPosition.z); }

            ReportPosition(PositionUtils.UnityToWorldPosition(currentPosition));

            return (characterController.isGrounded, deltaPosition, collisionFlags.HasFlag(CollisionFlags.Sides));
        }

        public void SetForward(Vector3 forward)
        {
            transform.forward = forward;
            CommonScriptableObjects.characterForward.Set(forward);
            CommonScriptableObjects.playerUnityEulerAngles.Set(transform.eulerAngles);
        }

        public Vector3 GetSpherecastPosition() =>
            transform.position;

        public Vector3 GetPosition() =>
            transform.position;

        public (Vector3 center, float radius, float skinWidth, float height) GetCharacterControllerSettings() =>
            (characterController.center, characterController.radius, characterController.skinWidth, characterController.height);

    }

    public interface ICharacterView
    {
        (bool isGrounded, Vector3 deltaPosition, bool wallHit) Move(Vector3 delta);

        void SetForward(Vector3 forward);

        Vector3 GetPosition();

        (Vector3 center, float radius, float skinWidth, float height) GetCharacterControllerSettings();

        Vector3 GetSpherecastPosition();
    }
}
