using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.Helpers;
using UnityEngine;

namespace ECSSystems.PlayerSystem
{
    public static class ECSPlayerSystem
    {

        private static readonly BaseVariable<Transform> avatarTransform = DataStore.i.world.avatarTransform;

        public static void Update()
        {
            if (!CommonScriptableObjects.rendererState.Get())
                return;

            Transform avatarT = avatarTransform.Get();

            UnityEngine.Vector3 avatarPosition = avatarT.position;
            Quaternion avatarRotation = avatarT.rotation;
            UnityEngine.Vector3 worldOffset = CommonScriptableObjects.worldOffset;

            var loadedScenes = ReferencesContainer.loadedScenes;
            var componentsWriter = ReferencesContainer.componentsWriter;

            IParcelScene scene;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                var transform = TransformHelper.SetTransform(scene, ref avatarPosition, ref avatarRotation, ref worldOffset);
                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.PLAYER_ENTITY, ComponentID.TRANSFORM,
                    transform, ECSComponentWriteType.SEND_TO_SCENE);
            }
        }
    }
}