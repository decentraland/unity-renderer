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

        private Vector3 lastAvatarPosition = Vector3.zero;
        private Quaternion lastAvatarRotation = Quaternion.identity;
        private int lamportTimestamp = 0;
        private bool newSceneAdded = false;

        public ECSPlayerTransformSystem(IECSComponentWriter componentsWriter,
            BaseList<IParcelScene> sdk7Scenes, BaseVariable<Transform> avatarTransform, Vector3Variable worldOffset)
        {
            loadedScenes = sdk7Scenes;
            this.avatarTransform = avatarTransform;
            this.worldOffset = worldOffset;
            this.componentsWriter = componentsWriter;

            loadedScenes.OnAdded += OnSceneLoaded;
        }

        public void Dispose()
        {
            loadedScenes.OnAdded -= OnSceneLoaded;
        }

        public void Update()
        {
            var currentAvatarTransform = avatarTransform.Get();

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
            Vector3 currentWorldOffset = this.worldOffset.Get();

            IParcelScene scene;

            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                var transform = TransformHelper.SetTransform(scene, ref avatarPosition, ref avatarRotation, ref currentWorldOffset);

                componentsWriter.PutComponent(scene.sceneData.sceneNumber, SpecialEntityId.PLAYER_ENTITY, ComponentID.TRANSFORM,
                    transform, lamportTimestamp, ECSComponentWriteType.SEND_TO_SCENE);

                componentsWriter.PutComponent(scene.sceneData.sceneNumber, SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION, ComponentID.TRANSFORM,
                    transform, ECSComponentWriteType.EXECUTE_LOCALLY);
            }

            lamportTimestamp++;
        }

        private void OnSceneLoaded(IParcelScene obj)
        {
            newSceneAdded = true;
        }
    }
}
