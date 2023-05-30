using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7
{
    public class SceneStateHandler : ISceneStateHandler
    {
        private IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
        private IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingState;
        private IReadOnlyDictionary<int, IParcelScene> scenes;

        public SceneStateHandler(
            IReadOnlyDictionary<int, IParcelScene> scenes,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent,
            IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingState)
        {
            this.scenes = scenes;
            this.engineInfoComponent = engineInfoComponent;
            this.gltfContainerLoadingState = gltfContainerLoadingState;
        }

        public uint GetSceneTick(int sceneNumber)
        {
            if (scenes.TryGetValue(sceneNumber, out var scene))
            {
                var model = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)?.model;

                if (model == null)
                {
                    model = InitializeEngineInfoComponentModel();
                    engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
                }

                return model.SceneTick;
            }

            return 0;
        }

        public void IncreaseSceneTick(int sceneNumber)
        {
            if (!scenes.TryGetValue(sceneNumber, out var scene)) return;

            var model = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)?.model?? InitializeEngineInfoComponentModel();
            model.SceneTick++;
            engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
        }

        public bool IsSceneGltfLoadingFinished(int sceneNumber)
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

        private InternalEngineInfo InitializeEngineInfoComponentModel() =>
            new InternalEngineInfo()
            {
                SceneTick = 0,
                SceneInitialRunTime = Time.realtimeSinceStartup
            };
    }
}
