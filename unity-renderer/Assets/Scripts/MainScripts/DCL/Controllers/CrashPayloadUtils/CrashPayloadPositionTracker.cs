using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Helpers
{
    public class CrashPayloadPositionTracker : IDisposable
    {
        public List<Vector3> movePositions = new List<Vector3>();
        public List<Vector3> teleportPositions = new List<Vector3>();

        private float lastPositionUpdate;
        private float positionUpdateInterval = 30.0f;
        private int maxQueueSize = 50;

        public CrashPayloadPositionTracker ()
        {
            CommonScriptableObjects.playerWorldPosition.OnChange += OnPositionChange;
            DataStore.i.player.lastTeleportPosition.OnChange += OnTeleport;
        }

        public void Dispose()
        {
            CommonScriptableObjects.playerWorldPosition.OnChange -= OnPositionChange;
            DataStore.i.player.lastTeleportPosition.OnChange -= OnTeleport;
        }

        private void OnTeleport(Vector3 current, Vector3 previous)
        {
            teleportPositions.Add( current );

            if ( teleportPositions.Count > maxQueueSize )
                teleportPositions.RemoveAt(0);
        }

        private void OnPositionChange(Vector3 current, Vector3 previous)
        {
            if ( lastPositionUpdate + positionUpdateInterval > Time.time )
            {
                movePositions.Add( current );
                lastPositionUpdate = Time.time;

                if ( movePositions.Count > maxQueueSize )
                    movePositions.RemoveAt(0);
            }
        }
    }
}