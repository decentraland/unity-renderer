using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.Models;
using ECSSystems.Helpers;
using UnityEngine;

namespace ECSSystems.CameraSystem
{
    public static class ECSCameraSystem
    {
        private static ECSTransform reusableTransform = new ECSTransform()
        {
            position = UnityEngine.Vector3.zero,
            scale = UnityEngine.Vector3.one,
            parentId = SpecialEntityId.SCENE_ROOT_ENTITY,
            rotation = Quaternion.identity
        };

        private static DataStore_Camera dataStoreCamera = DataStore.i.camera;

        public static void Update()
        {
            Transform cameraT = dataStoreCamera.transform.Get();

            UnityEngine.Vector3 cameraPosition = cameraT.position;
            Quaternion cameraRotation = cameraT.rotation;
            UnityEngine.Vector3 worldOffset = CommonScriptableObjects.worldOffset;

            var loadedScenes = ReferencesContainer.loadedScenes;
            var componentsWriter = ReferencesContainer.componentsWriter;

            IParcelScene scene;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                SetTransform(scene, ref cameraPosition, ref cameraRotation, ref worldOffset);
                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.CAMERA_ENTITY, ComponentID.TRANSFORM, reusableTransform);
            }
        }

        private static void SetTransform(IParcelScene scene, ref UnityEngine.Vector3 cameraPosition,
            ref Quaternion cameraRotation, ref UnityEngine.Vector3 worldOffset)
        {
            reusableTransform.position.x = (cameraPosition.x + worldOffset.x) - scene.sceneData.basePosition.x * ParcelSettings.PARCEL_SIZE;
            reusableTransform.position.y = (cameraPosition.y + worldOffset.y);
            reusableTransform.position.z = (cameraPosition.z + worldOffset.z) - scene.sceneData.basePosition.y * ParcelSettings.PARCEL_SIZE;

            reusableTransform.rotation.x = cameraRotation.x;
            reusableTransform.rotation.y = cameraRotation.y;
            reusableTransform.rotation.z = cameraRotation.z;
            reusableTransform.rotation.w = cameraRotation.w;
        }
    }
}