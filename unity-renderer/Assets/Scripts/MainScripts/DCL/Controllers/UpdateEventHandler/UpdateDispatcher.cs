using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class UpdateDispatcher : MonoBehaviour
    {
        public class UpdateEventCollection
        {
            public HashSet<Action> eventHashset = new HashSet<Action>();
            public List<Action> eventList = new List<Action>();

            public void Add(Action action)
            {
                if ( eventHashset.Contains(action))
                    return;

                eventList.Add(action);
                eventHashset.Add(action);
            }

            public void Remove(Action action)
            {
                if ( !eventHashset.Contains(action))
                    return;

                eventList.Remove(action);
                eventHashset.Remove(action);
            }

            public bool Contains(Action action)
            {
                return eventHashset.Contains(action);
            }
        }

        public Dictionary<IUpdateEventHandler.EventType, UpdateEventCollection> eventCollections = new Dictionary<IUpdateEventHandler.EventType, UpdateEventCollection>();

        void Awake()
        {
            EnsureEventType(IUpdateEventHandler.EventType.Update);
            EnsureEventType(IUpdateEventHandler.EventType.LateUpdate);
            EnsureEventType(IUpdateEventHandler.EventType.FixedUpdate);
            EnsureEventType(IUpdateEventHandler.EventType.OnGui);
        }

        void EnsureEventType( IUpdateEventHandler.EventType eventType )
        {
            if ( !eventCollections.ContainsKey(eventType) )
                eventCollections.Add(eventType, new UpdateEventCollection());
        }

        void Dispatch( IUpdateEventHandler.EventType eventType )
        {
            var list = eventCollections[eventType].eventList;
            int count = eventCollections[eventType].eventList.Count;

            for ( int i = 0; i < count; i++ )
            {
                list[i].Invoke();
            }
        }

        void Update()
        {
            Dispatch(IUpdateEventHandler.EventType.Update);
        }

        void LateUpdate()
        {
            Dispatch(IUpdateEventHandler.EventType.LateUpdate);
        }

        void FixedUpdate()
        {
            Dispatch(IUpdateEventHandler.EventType.FixedUpdate);
        }

        void OnGUI()
        {
            Dispatch(IUpdateEventHandler.EventType.OnGui);
        }
    }
}