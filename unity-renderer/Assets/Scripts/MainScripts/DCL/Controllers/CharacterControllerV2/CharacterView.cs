using Cinemachine;
using DCL;
using DCL.Helpers;
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

        [Header("Data")]
        [SerializeField] private CharacterControllerData data;

        [Header("InputActions")]
        [SerializeField] private InputAction_Hold jumpAction;
        [SerializeField] private InputAction_Hold sprintAction;
        [SerializeField] private InputAction_Measurable characterYAxis;
        [SerializeField] private InputAction_Measurable characterXAxis;

        [SerializeField] private Vector3Variable cameraForward;
        [SerializeField] private Vector3Variable cameraRight;

        [Header("Old References")]
        public GameObject avatarGameObject;
        public GameObject firstPersonCameraGameObject;

        private DCLCharacterControllerV2 controller;
        private readonly DataStore_Player dataStorePlayer = DataStore.i.player;

        private void Awake()
        {
            controller = new DCLCharacterControllerV2(this, data, jumpAction, sprintAction, characterXAxis, characterYAxis, cameraForward, cameraRight);

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
        }

        [Obsolete]
        private void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            Vector3 oldPos = this.transform.position;
            this.transform.position = CharacterGlobals.characterPosition.unityPosition;

            /*if (CinemachineCore.Instance.BrainCount > 0)
                CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera?.OnTargetObjectWarped(transform, transform.position - oldPos);*/
        }

        // sent by kernel
        [UsedImplicitly]
        public void Teleport(string teleportPayload)
        {
            var newPosition = Utils.FromJsonWithNulls<Vector3>(teleportPayload);
            dataStorePlayer.lastTeleportPosition.Set(newPosition, notifyEvent: true);


            CharacterGlobals.characterPosition.worldPosition = newPosition;
            Environment.i.platform.physicsSyncController?.MarkDirty();
            CommonScriptableObjects.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);
            dataStorePlayer.playerWorldPosition.Set(CharacterGlobals.characterPosition.worldPosition);
            Vector2Int playerPosition = Utils.WorldToGridPosition(CharacterGlobals.characterPosition.worldPosition);
            CommonScriptableObjects.playerCoords.Set(playerPosition);
            dataStorePlayer.playerGridPosition.Set(playerPosition);
            dataStorePlayer.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);
        }

        private void Update()
        {
            controller.Update(Time.deltaTime);

        }

        public CollisionFlags Move(Vector3 delta)
        {
            CollisionFlags collisionFlags = characterController.Move(delta);

            Vector3 transformPosition = transform.position;

            if (transformPosition.y < 0)
            {
                transform.position = new Vector3(transformPosition.x, 0, transformPosition.z);
            }
            return collisionFlags;
        }

        public void SetForward(Vector3 forward)
        {
            transform.forward = forward;
            CommonScriptableObjects.characterForward.Set(forward);
            CommonScriptableObjects.playerUnityEulerAngles.Set(transform.eulerAngles);
        }
    }

    public interface ICharacterView
    {
        CollisionFlags Move(Vector3 delta);

        void SetForward(Vector3 forward);
    }
}
