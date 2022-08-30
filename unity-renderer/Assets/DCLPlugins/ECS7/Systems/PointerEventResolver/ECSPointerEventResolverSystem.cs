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
            public readonly Dictionary<string, PBPointerEventsResult> scenesDict = new Dictionary<string, PBPointerEventsResult>();
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
            // We iterate over the pending event results and group them by scene
            int queueCount = state.pointerEventsQueue.Count;
            
            // If there is no pending events, we skip
            if (queueCount == 0)
                return;
            
            for(int i = 0; i <= queueCount; i++)
            {
                var rawPointerevent = state.pointerEventsQueue.Dequeue();
                // If the scene hasn't been added, we create it
                if(!state.scenesDict.TryGetValue(rawPointerevent.sceneId, out PBPointerEventsResult eventsResult))
                    eventsResult = new PBPointerEventsResult();
                
                // We add the pending event to the list
                eventsResult.Commands.Add(ConverToProto(rawPointerevent));
                state.scenesDict[rawPointerevent.sceneId] = eventsResult;
            }

            // We send the pointer events to each scene
            foreach (KeyValuePair<string, PBPointerEventsResult> entry in state.scenesDict)
            {
                state.componentsWriter.PutComponent(entry.Key, SpecialEntityId.SCENE_ROOT_ENTITY, ComponentID.POINTER_EVENTS_RESULT, entry.Value);
            }
            
            // We clear everything for the next frame
            state.pointerEventsQueue.Clear();
            state.scenesDict.Clear();
        }
        
        private static PointerCommand ConverToProto(PointerEvent pointerEvent)
        {
            PointerCommand eventsResult = new PointerCommand();
            eventsResult.Button = pointerEvent.button;
            eventsResult.Analog = pointerEvent.analog;
            eventsResult.Hit = pointerEvent.hit;
            eventsResult.State = pointerEvent.type;
            eventsResult.Timestamp = pointerEvent.timestamp;
            return eventsResult;
        }
    }
}