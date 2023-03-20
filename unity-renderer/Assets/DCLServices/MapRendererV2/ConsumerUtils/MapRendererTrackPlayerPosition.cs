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

            UpdateCameraPosition(cameraController, newPos);
        }

        public static void UpdateCameraPosition(IMapCameraController cameraController, Vector3 playerPos)
        {
            var newCoords = DCL.Helpers.Utils.WorldToGridPositionUnclamped(playerPos);
            cameraController.SetPosition(newCoords);
        }

        public void Dispose()
        {
            playerWorldPosition.OnChange -= OnPlayerPositionChanged;
        }
    }
}
