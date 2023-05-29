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
        private readonly DataStore_Player dataStorePlayer = DataStore.i.player;

        public CrashPayloadPositionTracker()
        {
            dataStorePlayer.playerGridPosition.OnChange += OnPositionChange;
            dataStorePlayer.lastTeleportPosition.OnChange += OnTeleport;
        }

        public void Dispose()
        {
            dataStorePlayer.playerGridPosition.OnChange -= OnPositionChange;
            dataStorePlayer.lastTeleportPosition.OnChange -= OnTeleport;
        }

        private void OnTeleport(Vector3 current, Vector3 previous)
        {
            teleportPositions.Add(current);

            if (teleportPositions.Count > maxQueueSize)
                teleportPositions.RemoveAt(0);
        }

        private void OnPositionChange(Vector2Int current, Vector2Int previous)
        {
            if (lastPositionUpdate + positionUpdateInterval > Time.time)
            {
                movePositions.Add(dataStorePlayer.playerWorldPosition.Get());
                lastPositionUpdate = Time.time;

                if (movePositions.Count > maxQueueSize)
                    movePositions.RemoveAt(0);
            }
        }
    }
}