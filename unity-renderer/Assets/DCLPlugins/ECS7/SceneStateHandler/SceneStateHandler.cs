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
        private readonly IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
        private readonly IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingState;
        private readonly CRDTServiceContext rpcCrdtContext;
        private readonly IReadOnlyDictionary<int, IParcelScene> scenes;

        public SceneStateHandler(
            CRDTServiceContext rpcCrdtContext,
            RestrictedActionsContext rpcRestrictedActionContext,
            IReadOnlyDictionary<int, IParcelScene> scenes,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingState)
        {
            this.rpcCrdtContext = rpcCrdtContext;
            this.scenes = scenes;
            this.engineInfoComponent = engineInfoComponent;
            this.gltfContainerLoadingState = gltfContainerLoadingState;

            rpcCrdtContext.GetSceneTick += GetSceneTick;
            rpcCrdtContext.IncreaseSceneTick += IncreaseSceneTick;
            rpcCrdtContext.IsSceneGltfLoadingFinished += IsSceneGltfLoadingFinished;
            rpcRestrictedActionContext.IsSceneRestrictedActionEnabled = IsSceneRestrictedActionEnabled;
        }

        public void Dispose()
        {
            rpcCrdtContext.GetSceneTick -= GetSceneTick;
            rpcCrdtContext.IncreaseSceneTick -= IncreaseSceneTick;
            rpcCrdtContext.IsSceneGltfLoadingFinished -= IsSceneGltfLoadingFinished;
        }

        public void InitializeEngineInfoComponent(int sceneNumber)
        {
            if (!scenes.TryGetValue(sceneNumber, out var scene)) return;

            engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY,
                new InternalEngineInfo(0, Time.realtimeSinceStartup));
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

        internal bool IsSceneRestrictedActionEnabled(int sceneNumber)
        {
            if (scenes == null)
                return false;

            if (scenes.TryGetValue(sceneNumber, out var scene))
            {
                InternalEngineInfo engineInfo = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)!.Value.model;
                uint sceneTick = engineInfo.SceneTick;

                const uint TICK_THRESHOLD = 2;

                return sceneTick != 0
                       && engineInfo.EnableRestrictedActionTick != 0
                       && sceneTick <= (engineInfo.EnableRestrictedActionTick + TICK_THRESHOLD);
            }

            return false;
        }
    }
}
