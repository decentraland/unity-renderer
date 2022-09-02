using System;
using System.Collections.Generic;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents.Events;
using PointerCommand = DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand;
using PointerEvent = DCLPlugins.ECSComponents.Events.PointerEvent;

namespace DCLPlugins.ECS7.Systems.PointerEventResolver
{
    public static class ECSPointerEventResolverSystem
    {
        // This represent the last N commands that will be sent as a result to the scene
        internal const int MAX_AMOUNT_OF_POINTER_EVENTS_SENT = 30;

        internal class State
        {
            public Queue<PointerEvent> pendingPointerEventsQueue;
            public Queue<PointerEvent> currentPointerEventsQueue;
            public IECSComponentWriter componentsWriter;
            public readonly Dictionary<string, PBPointerEventsResult> scenesDict = new Dictionary<string, PBPointerEventsResult>();
        }

        public static Action CreateSystem(IECSComponentWriter componentsWriter, Queue<PointerEvent> pointerEventsQueue)
        {
            var state = new State()
            {
                pendingPointerEventsQueue = pointerEventsQueue,
                currentPointerEventsQueue = new Queue<PointerEvent>(MAX_AMOUNT_OF_POINTER_EVENTS_SENT),
                componentsWriter = componentsWriter
            };
            return () => LateUpdate(state);
        }

        internal static void LateUpdate(State state)
        {
            var pendingPointerEventsQueue = state.pendingPointerEventsQueue;
            var pointerEventsQueue = state.currentPointerEventsQueue;
            var scenesDict = state.scenesDict;
            Queue<PointerEvent> newPointerEventsQueue = new Queue<PointerEvent>(MAX_AMOUNT_OF_POINTER_EVENTS_SENT);

            // We iterate over the pending event results and group them by scene
            int queueCount = pendingPointerEventsQueue.Count + pointerEventsQueue.Count;

            // If there is no pending events, we skip
            if (queueCount == 0)
                return;

            // If we have more pointerEvents than the max amount that we should send, we remove the olds ones
            int amountOfItemsToRemove = queueCount - MAX_AMOUNT_OF_POINTER_EVENTS_SENT;
            if (amountOfItemsToRemove > 0)
            {
                for (int i = 0; i < amountOfItemsToRemove; i++)
                {
                    if (pointerEventsQueue.Count > 0)
                        pointerEventsQueue.Dequeue();
                    else
                        pendingPointerEventsQueue.Dequeue();
                }
            }

            // We resolve the older pointer events first 
            int amountOfItems = pointerEventsQueue.Count;
            for (int i = 0; i < amountOfItems; i++)
            {
                PointerEvent rawPointerevent = pointerEventsQueue.Dequeue();
                ResolvePointerEvent(rawPointerevent, state, ref newPointerEventsQueue);
            }

            // We resolve the new ones after so we are ordering them correctly
            amountOfItems = pendingPointerEventsQueue.Count;
            for (int i = 0; i < amountOfItems; i++)
            {
                PointerEvent rawPointerevent = pendingPointerEventsQueue.Dequeue();
                ResolvePointerEvent(rawPointerevent, state, ref newPointerEventsQueue);
            }

            // We send the pointer events to each scene
            foreach (KeyValuePair<string, PBPointerEventsResult> entry in scenesDict)
            {
                state.componentsWriter.PutComponent(entry.Key, SpecialEntityId.SCENE_ROOT_ENTITY, ComponentID.POINTER_EVENTS_RESULT, entry.Value);
            }

            // We clear everything for the next frame
            state.currentPointerEventsQueue = newPointerEventsQueue;

            pendingPointerEventsQueue.Clear();
            scenesDict.Clear();
        }

        private static void ResolvePointerEvent(PointerEvent rawPointerevent, State state, ref Queue<PointerEvent> newPointerEventsQueue)
        {
            newPointerEventsQueue.Enqueue(rawPointerevent);
            
            // If the scene hasn't been added, we create it
            if (!state.scenesDict.TryGetValue(rawPointerevent.sceneId, out PBPointerEventsResult eventsResult))
                eventsResult = new PBPointerEventsResult();

            // We add the pending event to the list
            eventsResult.Commands.Add(ConverToProto(rawPointerevent));
            state.scenesDict[rawPointerevent.sceneId] = eventsResult;
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