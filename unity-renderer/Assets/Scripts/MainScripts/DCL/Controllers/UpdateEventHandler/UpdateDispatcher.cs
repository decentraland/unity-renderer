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
                int count = eventList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (eventList[i] == action)
                        return;
                }

                eventList.Add(action);
                eventHashset.Add(action);
            }

            public void Remove(Action action)
            {
                int count = eventList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (eventList[i] == action)
                    {
                        eventList.RemoveAt(i);
                        eventHashset.Remove(action);
                        break;
                    }
                }
            }

            public bool Contains(Action action)
            {
                int count = eventList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (eventList[i] == action)
                    {
                        return true;
                    }
                }
                return false;
            }

        }

        public Dictionary<IUpdateEventHandler.EventType, UpdateEventCollection> eventCollections = new Dictionary<IUpdateEventHandler.EventType, UpdateEventCollection>();

        void Awake()
        {
            EnsureEventType(IUpdateEventHandler.EventType.Update);
            EnsureEventType(IUpdateEventHandler.EventType.LateUpdate);
            EnsureEventType(IUpdateEventHandler.EventType.FixedUpdate);
            EnsureEventType(IUpdateEventHandler.EventType.OnDestroy);
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

        private void OnDestroy()
        {
            Dispatch(IUpdateEventHandler.EventType.OnDestroy);
        }
    }
}
