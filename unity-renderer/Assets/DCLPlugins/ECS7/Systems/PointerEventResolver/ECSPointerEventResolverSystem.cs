using System;
using System.Collections.Generic;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents.Events;
using PointerCommand = DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand;

namespace DCLPlugins.ECS7.Systems.PointerEventResolver
{
    public static class ECSPointerEventResolverSystem
    {
        private class State
        {
            public Queue<PointerEvent> pointerEventsQueue;
            public IECSComponentWriter componentsWriter;
        }

        public static Action CreateSystem(IECSComponentWriter componentsWriter, Queue<PointerEvent> pointerEventsQueue)
        {
            var state = new State()
            {
                pointerEventsQueue = pointerEventsQueue,
                componentsWriter = componentsWriter
            };
            return () => LateUpdate(state);
        }

        private static void LateUpdate(State state)
        {
            Dictionary<string, PBPointerEventsResult> scenesDict = new Dictionary<string, PBPointerEventsResult>();
            
            int queueCount = state.pointerEventsQueue.Count;
            for(int i = 0; i <= queueCount; i++)
            {
                var rawPointerevent = state.pointerEventsQueue.Dequeue();
                if(!scenesDict.TryGetValue(rawPointerevent.sceneId, out PBPointerEventsResult eventsResult))
                    eventsResult = new PBPointerEventsResult();
                
                eventsResult.Commands.Add(ConverToProto(rawPointerevent));
                scenesDict[rawPointerevent.sceneId] = eventsResult;
            }

            foreach (KeyValuePair<string, PBPointerEventsResult> entry in scenesDict)
            {
                state.componentsWriter.PutComponent(entry.Key, SpecialEntityId.SCENE_ROOT_ENTITY, ComponentID.ON_POINTER_UP_RESULT, entry.Value);
            }
        }
        
        private static PointerCommand ConverToProto(PointerEvent pointerEvent)
        {
            PointerCommand eventsResult = new PointerCommand();
            eventsResult.Button = pointerEvent.button;
            eventsResult.Analog = pointerEvent.analog;
            eventsResult.Hit = pointerEvent.hit;
            eventsResult.State = pointerEvent.state;
            eventsResult.Timestamp = pointerEvent.timestamp;
            return eventsResult;
        }
    }
}