using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using RPC.Context;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7
{
    public class SceneStateHandler : IDisposable
    {
        private IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
        private IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingState;
        private CRDTServiceContext context;
        private IReadOnlyDictionary<int, IParcelScene> scenes;

        public SceneStateHandler(
            CRDTServiceContext context,
            IReadOnlyDictionary<int, IParcelScene> scenes,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingState)
        {
            this.context = context;
            this.scenes = scenes;
            this.engineInfoComponent = engineInfoComponent;
            this.gltfContainerLoadingState = gltfContainerLoadingState;

            context.GetSceneTick += GetSceneTick;
            context.IncreaseSceneTick += IncreaseSceneTick;
            context.IsSceneGltfLoadingFinished += IsSceneGltfLoadingFinished;
        }

        internal uint GetSceneTick(int sceneNumber)
        {
            if (scenes.TryGetValue(sceneNumber, out var scene))
                return engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)?.model.SceneTick ?? 0;

            return 0;
        }

        internal void IncreaseSceneTick(int sceneNumber)
        {
            if (!scenes.TryGetValue(sceneNumber, out var scene)) return;

            var model = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)?.model;

            InternalEngineInfo finalModel = model.Value;
            finalModel.SceneTick++;

            engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, finalModel);
        }

        internal bool IsSceneGltfLoadingFinished(int sceneNumber)
        {
            if (scenes.TryGetValue(sceneNumber, out var scene))
            {
                var keys = scene.entities.Keys;
                foreach (long entityId in keys)
                {
                    var model = gltfContainerLoadingState.GetFor(scene, entityId)?.model;
                    if (model != null && model.Value.LoadingState == LoadingState.Loading)
                        return false;
                }
            }

            return true;
        }

        public void InitializeEngineInfoComponent(int sceneNumber)
        {
            if (!scenes.TryGetValue(sceneNumber, out var scene)) return;

            engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY,
                new InternalEngineInfo(0, Time.realtimeSinceStartup));
        }

        public void Dispose()
        {
            context.GetSceneTick -= GetSceneTick;
            context.IncreaseSceneTick -= IncreaseSceneTick;
            context.IsSceneGltfLoadingFinished -= IsSceneGltfLoadingFinished;
        }
    }
}
