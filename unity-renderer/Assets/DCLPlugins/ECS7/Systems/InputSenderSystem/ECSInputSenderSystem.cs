using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using System;
using System.Collections.Generic;

namespace ECSSystems.InputSenderSystem
{
    public static class ECSInputSenderSystem
    {
        private class State
        {
            public IInternalECSComponent<InternalInputEventResults> inputResultComponent;
            public IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
            public IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
            public WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>> componentPool;
            public uint lastTimestamp = 0;
        }

        public static Action CreateSystem(
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>> componentPool)
        {
            var state = new State()
            {
                inputResultComponent = inputResultComponent,
                engineInfoComponent = engineInfoComponent,
                componentsWriter = componentsWriter,
                componentPool = componentPool,
            };

            return () => Update(state);
        }

        private static void Update(State state)
        {
            var inputResults = state.inputResultComponent.GetForAll();

            for (int i = 0; i < inputResults.Count; i++)
            {
                var model = inputResults[i].value.model;

                if (!model.dirty)
                    continue;

                var scene = inputResults[i].value.scene;
                var entity = inputResults[i].value.entity;

                if (!state.componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                int count = model.events.Count;

                for (int j = 0; j < count; j++)
                {
                    InternalInputEventResults.EventData inputEvent = model.events[j];

                    var componentPooled = state.componentPool.Get();
                    var componentModel = componentPooled.WrappedComponent.Model;
                    componentModel.Button = inputEvent.button;
                    componentModel.Hit = inputEvent.hit;
                    componentModel.State = inputEvent.type;
                    componentModel.Timestamp = state.lastTimestamp++;
                    componentModel.TickNumber = state.engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).Value.model.SceneTick;

                    writer.Append(entity.entityId, ComponentID.POINTER_EVENTS_RESULT, componentPooled);
                }

                model.events.Clear();
            }
        }
    }
}
