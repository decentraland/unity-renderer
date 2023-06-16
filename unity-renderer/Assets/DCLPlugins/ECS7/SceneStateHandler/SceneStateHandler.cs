using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using RPC.Context;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7
{
    public class SceneStateHandler : IDisposable
    {
        private readonly IInternalECSComponent<InternalEngineInfo> internalEngineInfoComponent;
        private readonly IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingState;
        private readonly IInternalECSComponent<InternalIncreaseTickTagComponent> increaseSceneTickTagComponent;
        private readonly ECSComponent<PBEngineInfo> engineInfoComponent;
        private readonly CRDTServiceContext rpcCrdtContext;
        private IReadOnlyDictionary<int, IParcelScene> scenes;

        public SceneStateHandler(
            CRDTServiceContext rpcCrdtContext,
            RestrictedActionsContext rpcRestrictedActionContext,
            IReadOnlyDictionary<int, IParcelScene> scenes,
            IInternalECSComponent<InternalEngineInfo> internalEngineInfoComponent,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingState,
            IInternalECSComponent<InternalIncreaseTickTagComponent> increaseSceneTickTagComponent,
            ECSComponent<PBEngineInfo> engineInfoComponent)
        {
            this.rpcCrdtContext = rpcCrdtContext;
            this.scenes = scenes;
            this.internalEngineInfoComponent = internalEngineInfoComponent;
            this.gltfContainerLoadingState = gltfContainerLoadingState;
            this.increaseSceneTickTagComponent = increaseSceneTickTagComponent;
            this.engineInfoComponent = engineInfoComponent;

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

            internalEngineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY,
                new InternalEngineInfo()
                {
                    SceneTick = 0,
                    SceneInitialRunTime = Time.realtimeSinceStartup,
                    SceneInitialFrameCount = Time.frameCount
                });
        }

        internal uint GetSceneTick(int sceneNumber)
        {
            if (scenes.TryGetValue(sceneNumber, out var scene))
            {
                return engineInfoComponent.Get(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model.TickNumber;
            }

            return 0;
        }

        internal void IncreaseSceneTick(int sceneNumber)
        {
            increaseSceneTickTagComponent.PutFor(sceneNumber, SpecialEntityId.SCENE_ROOT_ENTITY,
                new InternalIncreaseTickTagComponent());
        }

        internal bool IsSceneGltfLoadingFinished(int sceneNumber)
        {
            if (scenes.TryGetValue(sceneNumber, out var scene))
            {
                var keys = scene.entities.Keys;

                foreach (long entityId in keys)
                {
                    var model = gltfContainerLoadingState.GetFor(scene, entityId)?.model;

                    if (model != null && model.LoadingState == LoadingState.Loading)
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
                InternalEngineInfo engineInfo = internalEngineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model;
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
