using System;
using System.Collections.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
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
            PBPointerEventsResult eventsResult = new PBPointerEventsResult();

            int queueCount = state.pointerEventsQueue.Count;
            for(int i = 0; i <= queueCount; i++)
            {
                var rawPointerevent = state.pointerEventsQueue.Dequeue();
                eventsResult.Commands.Add(ConverToProto(rawPointerevent));
            }
            
            // state.componentsWriter.PutComponent();
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