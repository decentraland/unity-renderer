using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class UpdateEventHandler : IUpdateEventHandler
    {
        private UpdateDispatcher dispatcher;

        public UpdateEventHandler ()
        {
            var go = new GameObject("_UpdateDispatcher");
            dispatcher = go.AddComponent<UpdateDispatcher>();
        }

        public void AddListener( IUpdateEventHandler.EventType eventType, Action action )
        {
            Assert.IsTrue( dispatcher != null, "Dispatcher is null! This should never happen!");
            dispatcher.eventCollections[eventType].Add(action);
        }

        public void RemoveListener( IUpdateEventHandler.EventType eventType, Action action )
        {
            Assert.IsTrue( dispatcher != null, "Dispatcher is null! This should never happen!");
            dispatcher.eventCollections[eventType].Remove(action);
        }

        public void Dispose()
        {
            if ( dispatcher != null )
            {
                UnityEngine.Object.Destroy( dispatcher.gameObject );
                dispatcher = null;
            }
        }
    }
}