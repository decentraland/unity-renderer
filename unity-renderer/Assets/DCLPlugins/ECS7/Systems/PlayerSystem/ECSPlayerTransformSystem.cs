using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.PlayerSystem
{
    public class ECSPlayerTransformSystem : IDisposable
    {
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly BaseVariable<Transform> avatarTransform;
        private readonly Vector3Variable worldOffset;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool;
        private readonly ECSComponent<ECSTransform> transformComponent;

        private Vector3 lastAvatarPosition = Vector3.zero;
        private Quaternion lastAvatarRotation = Quaternion.identity;
        private bool newSceneAdded = false;

        public ECSPlayerTransformSystem(IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool,
            ECSComponent<ECSTransform> transformComponent,
            BaseList<IParcelScene> sdk7Scenes, BaseVariable<Transform> avatarTransform, Vector3Variable worldOffset)
        {
            loadedScenes = sdk7Scenes;
            this.avatarTransform = avatarTransform;
            this.worldOffset = worldOffset;
            this.componentsWriter = componentsWriter;
            this.transformPool = transformPool;
            this.transformComponent = transformComponent;

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

                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                var pooledTransform = transformPool.Get();
                var transform = pooledTransform.WrappedComponent.Model;
                transform.position = UtilsScene.GlobalToScenePosition(ref scene.sceneData.basePosition, ref avatarPosition, ref currentWorldOffset);
                transform.rotation = avatarRotation;

                writer.Put(SpecialEntityId.PLAYER_ENTITY, ComponentID.TRANSFORM, pooledTransform);

                if (scene.entities.TryGetValue(SpecialEntityId.PLAYER_ENTITY, out var entity))
                {
                    ECSTransform stored = transformComponent.Get(scene, entity.entityId)?.model ?? new ECSTransform();
                    stored.position = transform.position;
                    stored.rotation = transform.rotation;
                    stored.scale = transform.scale;
                    stored.parentId = transform.parentId;
                    transformComponent.SetModel(scene, entity, stored);
                }
            }
        }

        private void OnSceneLoaded(IParcelScene obj)
        {
            newSceneAdded = true;
        }

        private static Vector3 SetInSceneOffset(IParcelScene scene, ref Vector3 position, ref Vector3 worldOffset)
        {
            Vector3 offsetposition = new Vector3();
            offsetposition.x = (position.x + worldOffset.x) - scene.sceneData.basePosition.x * ParcelSettings.PARCEL_SIZE;
            offsetposition.y = (position.y + worldOffset.y);
            offsetposition.z = (position.z + worldOffset.z) - scene.sceneData.basePosition.y * ParcelSettings.PARCEL_SIZE;
            return offsetposition;
        }
    }
}
