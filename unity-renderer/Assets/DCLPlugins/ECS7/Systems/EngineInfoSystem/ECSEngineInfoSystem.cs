using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace ECSSystems.ECSEngineInfoSystem
{
    /// <summary>
    /// This system updates every scene's root entity EngineInfo component on every frame
    /// </summary>
    public class ECSEngineInfoSystem
    {
        private readonly BaseList<IParcelScene> scenes;
        private readonly IECSComponentWriter componentWriter;
        private readonly IInternalECSComponent<InternalEngineInfo> internalEngineInfo;

        public ECSEngineInfoSystem(
            BaseList<IParcelScene> scenes,
            IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalEngineInfo> internalEngineInfo)
        {
            this.scenes = scenes;
            this.componentWriter = componentWriter;
            this.internalEngineInfo = internalEngineInfo;
        }

        public void Update()
        {
            int currentEngineFrameCount = Time.frameCount;
            float currentEngineRunTime = Time.realtimeSinceStartup;

            // Internal EngineInfo component is initialized and attached by SceneStateHandler when a scene is loaded
            var componentGroup = internalEngineInfo.GetForAll();

            int entitiesCount = componentGroup.Count;
            for (int i = 0; i < entitiesCount; i++)
            {
                var entity = componentGroup[i].value.entity;
                var scene = componentGroup[i].value.scene;
                var model = componentGroup[i].value.model;

                model.SceneTick++;

                componentWriter.PutComponent(
                    entity.scene,
                    scene.GetEntityById(SpecialEntityId.SCENE_ROOT_ENTITY),
                    ComponentID.ENGINE_INFO,
                    new PBEngineInfo()
                    {
                        TickNumber = model.SceneTick,
                        FrameNumber = (uint)currentEngineFrameCount,
                        TotalRuntime = currentEngineRunTime - model.SceneInitialRunTime
                    });

                internalEngineInfo.PutFor(scene, entity, model);
            }
        }
    }
}
