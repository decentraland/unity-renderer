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
            // Traverse all scenes and add/update tick, frame, scene time
            // int scenesAmount = scenes.Count;
            // for (int i = 0; i < scenesAmount; i++)
            // {
            //     var scene = scenes[i];
            //     // scenes[i]
            //     componentWriter.PutComponent(
            //         scene,
            //         scene.GetEntityById(SpecialEntityId.SCENE_ROOT_ENTITY),
            //         ComponentID.ENGINE_INFO,
            //         new PBEngineInfo()
            //         {
            //
            //         }
            //         );
            // }

            int currentEngineFrameCount = Time.frameCount;
            float currentEngineRunTime = Time.realtimeSinceStartup;

            var componentGroup = internalEngineInfo.GetForAll();
            int entitiesCount = componentGroup.Count;
            for (int i = 0; i < entitiesCount; i++)
            {
                var entity = componentGroup[i].value.entity;
                var scene = componentGroup[i].value.scene;
                var model = componentGroup[i].value.model;

                // TODO: How to deal with this happening in the SceneStateHandler ???
                model.SceneTick++;

                componentWriter.PutComponent(
                    entity.scene,
                    scene.GetEntityById(SpecialEntityId.SCENE_ROOT_ENTITY),
                    ComponentID.ENGINE_INFO,
                    new PBEngineInfo()
                    {
                        TickNumber = model.SceneTick,
                        FrameNumber = (uint)(currentEngineFrameCount - model.SceneInitialFrameNumber),
                        TotalRuntime = currentEngineRunTime - model.SceneInitialRunTime
                    });

                internalEngineInfo.PutFor(scene, entity, model);
            }
        }
    }
}
