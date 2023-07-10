using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.ECSEngineInfoSystem
{
    /// <summary>
    /// This system updates every scene's root entity EngineInfo component on every frame
    /// </summary>
    public class ECSEngineInfoSystem
    {
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly IInternalECSComponent<InternalEngineInfo> internalEngineInfo;
        private readonly WrappedComponentPool<IWrappedComponent<PBEngineInfo>> componentPool;

        public ECSEngineInfoSystem(
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBEngineInfo>> componentPool,
            IInternalECSComponent<InternalEngineInfo> internalEngineInfo)
        {
            this.componentsWriter = componentsWriter;
            this.internalEngineInfo = internalEngineInfo;
            this.componentPool = componentPool;
        }

        public void Update()
        {
            int currentEngineFrameCount = Time.frameCount;
            float currentEngineRunTime = Time.realtimeSinceStartup;

            // Scenes internal EngineInfo component is initialized at SceneStateHandler.InitializeEngineInfoComponent()
            var componentGroup = internalEngineInfo.GetForAll();

            int entitiesCount = componentGroup.Count;

            for (int i = 0; i < entitiesCount; i++)
            {
                var scene = componentGroup[i].value.scene;
                var model = componentGroup[i].value.model;

                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                var componentPooled = componentPool.Get();
                var componentModel = componentPooled.WrappedComponent.Model;
                componentModel.TickNumber = model.SceneTick;
                componentModel.FrameNumber = (uint)(currentEngineFrameCount - model.SceneInitialRunTime);
                componentModel.TotalRuntime = currentEngineRunTime - model.SceneInitialRunTime;

                writer.Put(SpecialEntityId.SCENE_ROOT_ENTITY, ComponentID.ENGINE_INFO, componentPooled);
            }
        }
    }
}
