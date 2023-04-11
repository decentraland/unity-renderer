using DCLServices.MapRendererV2.MapCameraController;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.ConsumerUtils
{
    /// <summary>
    /// Updates Map Camera Controller's position accordingly to the player position
    /// </summary>
    public struct MapRendererTrackPlayerPosition : IDisposable
    {
        private const float SQR_DISTANCE_TOLERANCE = 0.01f;

        private readonly IMapCameraController cameraController;
        private readonly BaseVariable<Vector3> playerWorldPosition;

        private Vector3 lastPlayerPosition;

        public MapRendererTrackPlayerPosition(IMapCameraController cameraController, BaseVariable<Vector3> playerWorldPosition)
        {
            this.cameraController = cameraController;
            this.playerWorldPosition = playerWorldPosition;

            lastPlayerPosition = Vector3.positiveInfinity;

            OnPlayerPositionChanged(Vector3.positiveInfinity, playerWorldPosition.Get());

            playerWorldPosition.OnChange += OnPlayerPositionChanged;
        }

        private void OnPlayerPositionChanged(Vector3 oldPos, Vector3 newPos)
        {
            if (Vector3.SqrMagnitude(newPos - lastPlayerPosition) < SQR_DISTANCE_TOLERANCE)
                return;

            lastPlayerPosition = newPos;
            cameraController.SetPosition(GetPlayerCentricCoords(newPos));
        }

        public static Vector2 GetPlayerCentricCoords(Vector3 playerPosition)
        {
            var newCoords = DCL.Helpers.Utils.WorldToGridPositionUnclamped(playerPosition);
            return GetPlayerCentricCoords(newCoords);
        }

        public static Vector2 GetPlayerCentricCoords(Vector2 playerCoords)
        {
            // quick hack to align with `CoordsToPositionWithOffset` and the pivot
            return playerCoords - Vector2.one;
        }

        public void Dispose()
        {
            playerWorldPosition.OnChange -= OnPlayerPositionChanged;
        }
    }
}
