using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.PlayerSystem
{
    public class ECSPlayerTransformSystem : IDisposable
    {
        private readonly BaseVariable<Transform> avatarTransform;
        private readonly RendererState rendererState;
        private readonly Vector3Variable worldOffset;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly IECSComponentWriter componentsWriter;

        private Vector3 lastAvatarPosition = Vector3.zero;
        private Quaternion lastAvatarRotation = Quaternion.identity;
        private long timeStamp = 0;
        private bool newSceneAdded = false;

        public ECSPlayerTransformSystem(IECSComponentWriter componentsWriter)
        {
            avatarTransform = DataStore.i.world.avatarTransform;
            rendererState = CommonScriptableObjects.rendererState;
            worldOffset = CommonScriptableObjects.worldOffset;
            loadedScenes = DataStore.i.ecs7.scenes;
            this.componentsWriter = componentsWriter;

            loadedScenes.OnAdded += LoadedScenesOnOnAdded;
        }

        public void Dispose()
        {
            loadedScenes.OnAdded -= LoadedScenesOnOnAdded;
        }

        public void Update()
        {
            if (!rendererState.Get())
                return;

            Transform avatarT = avatarTransform.Get();

            Vector3 avatarPosition = avatarT.position;
            Quaternion avatarRotation = avatarT.rotation;

            if (!newSceneAdded && lastAvatarPosition == avatarPosition && lastAvatarRotation == avatarRotation)
            {
                return;
            }

            newSceneAdded = false;

            lastAvatarPosition = avatarPosition;
            lastAvatarRotation = avatarRotation;
            Vector3 worldOffset = this.worldOffset.Get();

            IReadOnlyList<IParcelScene> loadedScenes = this.loadedScenes;
            var componentsWriter = this.componentsWriter;

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

        private void LoadedScenesOnOnAdded(IParcelScene obj)
        {
            newSceneAdded = true;
        }
    }
}
