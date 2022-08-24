using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.Helpers;
using UnityEngine;

namespace ECSSystems.PlayerSystem
{
    public static class ECSPlayerTransformSystem
    {
        private class State
        {
            public BaseVariable<Transform> avatarTransform;
            public RendererState rendererState;
            public Vector3Variable worldOffset;
            public IReadOnlyList<IParcelScene> loadedScenes;
            public IECSComponentWriter componentsWriter;
            public UnityEngine.Vector3 lastAvatarPosition = UnityEngine.Vector3.zero;
            public Quaternion lastAvatarRotation = Quaternion.identity;
            public long timeStamp = 0;
        }

        public static Action CreateSystem(IECSComponentWriter componentsWriter)
        {
            var state = new State()
            {
                avatarTransform = DataStore.i.world.avatarTransform,
                rendererState = CommonScriptableObjects.rendererState,
                worldOffset = CommonScriptableObjects.worldOffset,
                loadedScenes = DataStore.i.ecs7.scenes,
                componentsWriter = componentsWriter
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            if (!state.rendererState.Get())
                return;

            Transform avatarT = state.avatarTransform.Get();

            UnityEngine.Vector3 avatarPosition = avatarT.position;
            Quaternion avatarRotation = avatarT.rotation;

            if (state.lastAvatarPosition == avatarPosition && state.lastAvatarRotation == avatarRotation)
            {
                return;
            }

            state.lastAvatarPosition = avatarPosition;
            state.lastAvatarRotation = avatarRotation;
            UnityEngine.Vector3 worldOffset = state.worldOffset.Get();

            var loadedScenes = state.loadedScenes;
            var componentsWriter = state.componentsWriter;

            IParcelScene scene;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                var transform = TransformHelper.SetTransform(scene, ref avatarPosition, ref avatarRotation, ref worldOffset);

                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.PLAYER_ENTITY, ComponentID.TRANSFORM,
                    transform, state.timeStamp, ECSComponentWriteType.SEND_TO_SCENE);

                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION, ComponentID.TRANSFORM,
                    transform, ECSComponentWriteType.EXECUTE_LOCALLY);
            }
            state.timeStamp++;
        }
    }
}