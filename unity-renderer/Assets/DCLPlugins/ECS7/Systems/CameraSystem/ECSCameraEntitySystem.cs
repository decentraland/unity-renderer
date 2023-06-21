using DCL.CameraTool;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
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

        private Vector3 lastCameraPosition = Vector3.zero;
        private Quaternion lastCameraRotation = Quaternion.identity;
        private bool newSceneAdded = false;

        public ECSCameraEntitySystem(IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBCameraMode>> cameraModePool,
            WrappedComponentPool<IWrappedComponent<PBPointerLock>> pointerLockPool,
            WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool,
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
            var cameraModeWrappedComponent = cameraModePool.GetElement();
            var pooledWrappedComponent = pointerLockPool.GetElement();
            cameraModeWrappedComponent.WrappedComponent.Model.Mode = ProtoConvertUtils.UnityEnumToPBCameraEnum(currentCameraMode);
            pooledWrappedComponent.WrappedComponent.Model.IsPointerLocked = Utils.IsCursorLocked;

            Vector3 currentWorldOffset = worldOffset.Get();
            IReadOnlyList<IParcelScene> loadedScenes = this.loadedScenes;

            IParcelScene scene;

            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out ComponentWriter writer))
                    continue;

                writer.Put(SpecialEntityId.CAMERA_ENTITY, ComponentID.CAMERA_MODE, cameraModeWrappedComponent);
                writer.Put(SpecialEntityId.CAMERA_ENTITY, ComponentID.POINTER_LOCK, pooledWrappedComponent);

                if (!updateTransform)
                    continue;

                var pooledTransformComponent = transformPool.GetElement();
                var t = pooledTransformComponent.WrappedComponent.Model;
                t.position = SetInSceneOffset(scene, ref cameraPosition, ref currentWorldOffset);
                t.rotation = cameraRotation;

                writer.Put(SpecialEntityId.CAMERA_ENTITY, ComponentID.TRANSFORM, pooledTransformComponent);
            }
        }

        private void LoadedScenesOnOnAdded(IParcelScene obj)
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
