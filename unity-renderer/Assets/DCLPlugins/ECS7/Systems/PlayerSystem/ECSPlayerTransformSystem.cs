using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.Helpers;
using System;
using UnityEngine;

namespace ECSSystems.PlayerSystem
{
    public class ECSPlayerTransformSystem : IDisposable
    {
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly BaseVariable<Transform> avatarTransform;
        private readonly Vector3Variable worldOffset;
        private readonly IECSComponentWriter componentsWriter;

        private Transform currentAvatarTransform = null;
        private Vector3 currentWorldOffset = Vector3.zero;

        private Vector3 lastAvatarPosition = Vector3.zero;
        private Quaternion lastAvatarRotation = Quaternion.identity;
        private long timeStamp = 0;
        private bool newSceneAdded = false;

        // TODO: dependency injection, camera syste refactor like this one
        public ECSPlayerTransformSystem(IECSComponentWriter componentsWriter,
            BaseList<IParcelScene> sdk7Scenes, BaseVariable<Transform> avatarTransform, Vector3Variable worldOffset)
        {
            loadedScenes = sdk7Scenes;
            this.avatarTransform = avatarTransform;
            this.worldOffset = worldOffset;
            this.componentsWriter = componentsWriter;

            loadedScenes.OnAdded += OnSceneLoaded;
            avatarTransform.OnChange += OnAvatarTransformChanged;
            worldOffset.OnChange += OnWorldOffsetChanged;

            OnAvatarTransformChanged(DataStore.i.world.avatarTransform.Get(), null);
            OnWorldOffsetChanged(CommonScriptableObjects.worldOffset.Get(), Vector3.zero);
        }

        public void Dispose()
        {
            loadedScenes.OnAdded -= OnSceneLoaded;
            avatarTransform.OnChange -= OnAvatarTransformChanged;
            worldOffset.OnChange -= OnWorldOffsetChanged;
        }

        public void Update()
        {
            if (currentAvatarTransform == null)
                return;

            Vector3 avatarPosition = currentAvatarTransform.position;
            Quaternion avatarRotation = currentAvatarTransform.rotation;

            if (!newSceneAdded && lastAvatarPosition == avatarPosition && lastAvatarRotation == avatarRotation)
            {
                return;
            }

            newSceneAdded = false;

            lastAvatarPosition = avatarPosition;
            lastAvatarRotation = avatarRotation;
            Vector3 worldOffset = currentWorldOffset;

            IParcelScene scene;

            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                var transform = TransformHelper.SetTransform(scene, ref avatarPosition, ref avatarRotation, ref worldOffset);

                componentsWriter.PutComponent(scene.sceneData.sceneNumber, SpecialEntityId.PLAYER_ENTITY, ComponentID.TRANSFORM,
                    transform, timeStamp, ECSComponentWriteType.SEND_TO_SCENE);

                componentsWriter.PutComponent(scene.sceneData.sceneNumber, SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION, ComponentID.TRANSFORM,
                    transform, ECSComponentWriteType.EXECUTE_LOCALLY);
            }

            timeStamp++;
        }

        private void OnSceneLoaded(IParcelScene obj)
        {
            newSceneAdded = true;
        }

        private void OnAvatarTransformChanged(Transform current, Transform previous)
        {
            currentAvatarTransform = current;
        }

        private void OnWorldOffsetChanged(Vector3 current, Vector3 previous)
        {
            currentWorldOffset = current;
        }
    }
}
