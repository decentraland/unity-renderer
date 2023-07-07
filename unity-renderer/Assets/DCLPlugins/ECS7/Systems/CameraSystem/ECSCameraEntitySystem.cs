using DCL.CameraTool;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.CameraSystem
{
    public class ECSCameraEntitySystem : IDisposable
    {
        private readonly WrappedComponentPool<IWrappedComponent<PBCameraMode>> cameraModePool;
        private readonly WrappedComponentPool<IWrappedComponent<PBPointerLock>> pointerLockPool;
        private readonly WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool;
        private readonly BaseVariable<Transform> cameraTransform;
        private readonly Vector3Variable worldOffset;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly BaseVariableAsset<CameraMode.ModeId> cameraMode;
        private readonly ECSComponent<ECSTransform> transformComponent;

        private Vector3 lastCameraPosition = Vector3.zero;
        private Quaternion lastCameraRotation = Quaternion.identity;
        private bool newSceneAdded = false;

        public ECSCameraEntitySystem(IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBCameraMode>> cameraModePool,
            WrappedComponentPool<IWrappedComponent<PBPointerLock>> pointerLockPool,
            WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool,
            ECSComponent<ECSTransform> transformComponent,
            BaseList<IParcelScene> loadedScenes, BaseVariable<Transform> cameraTransform, Vector3Variable worldOffset,
            BaseVariableAsset<CameraMode.ModeId> cameraMode)
        {
            this.cameraTransform = cameraTransform;
            this.worldOffset = worldOffset;
            this.loadedScenes = loadedScenes;
            this.componentsWriter = componentsWriter;
            this.cameraMode = cameraMode;
            this.cameraModePool = cameraModePool;
            this.pointerLockPool = pointerLockPool;
            this.transformPool = transformPool;
            this.transformComponent = transformComponent;

            loadedScenes.OnAdded += LoadedScenesOnOnAdded;
        }

        public void Dispose()
        {
            loadedScenes.OnAdded -= LoadedScenesOnOnAdded;
        }

        public void Update()
        {
            var currentCameraTransform = cameraTransform.Get();

            if (currentCameraTransform == null)
                return;

            Vector3 cameraPosition = currentCameraTransform.position;
            Quaternion cameraRotation = currentCameraTransform.rotation;

            bool updateTransform = newSceneAdded || (lastCameraPosition != cameraPosition || lastCameraRotation != cameraRotation);
            newSceneAdded = false;

            lastCameraPosition = cameraPosition;
            lastCameraRotation = cameraRotation;

            var currentCameraMode = cameraMode.Get();

            var cameraModeId = ProtoConvertUtils.UnityEnumToPBCameraEnum(currentCameraMode);
            var isCursorLocked = Utils.IsCursorLocked;

            Vector3 currentWorldOffset = worldOffset.Get();
            IReadOnlyList<IParcelScene> loadedScenes = this.loadedScenes;

            IParcelScene scene;

            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out ComponentWriter writer))
                    continue;

                var cameraModeWrappedComponent = cameraModePool.Get();
                var pooledWrappedComponent = pointerLockPool.Get();
                cameraModeWrappedComponent.WrappedComponent.Model.Mode = cameraModeId;
                pooledWrappedComponent.WrappedComponent.Model.IsPointerLocked = isCursorLocked;

                writer.Put(SpecialEntityId.CAMERA_ENTITY, ComponentID.CAMERA_MODE, cameraModeWrappedComponent);
                writer.Put(SpecialEntityId.CAMERA_ENTITY, ComponentID.POINTER_LOCK, pooledWrappedComponent);

                if (!updateTransform)
                    continue;

                var pooledTransformComponent = transformPool.Get();
                var t = pooledTransformComponent.WrappedComponent.Model;
                t.position = UtilsScene.GlobalToScenePosition(ref scene.sceneData.basePosition, ref cameraPosition, ref currentWorldOffset);
                t.rotation = cameraRotation;

                writer.Put(SpecialEntityId.CAMERA_ENTITY, ComponentID.TRANSFORM, pooledTransformComponent);

                if (scene.entities.TryGetValue(SpecialEntityId.CAMERA_ENTITY, out var entity))
                {
                    ECSTransform stored = transformComponent.Get(scene, entity.entityId)?.model ?? new ECSTransform();
                    stored.position = t.position;
                    stored.rotation = t.rotation;
                    stored.scale = t.scale;
                    stored.parentId = t.parentId;
                    transformComponent.SetModel(scene, entity, stored);
                }
            }
        }

        private void LoadedScenesOnOnAdded(IParcelScene obj)
        {
            newSceneAdded = true;
        }
    }
}
