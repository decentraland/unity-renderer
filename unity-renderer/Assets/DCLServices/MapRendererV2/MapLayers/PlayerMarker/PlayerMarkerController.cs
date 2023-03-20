using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    internal class PlayerMarkerController : MapLayerControllerBase, IMapLayerController
    {
        internal delegate IPlayerMarker PlayerMarkerBuilder(Transform parent);

        private readonly PlayerMarkerBuilder builder;
        private readonly BaseVariable<Vector3> playerWorldPosition;
        private readonly Vector3Variable playerRotation;

        private IPlayerMarker playerMarker;

        public PlayerMarkerController(
            PlayerMarkerBuilder builder,
            BaseVariable<Vector3> playerWorldPosition, Vector3Variable playerRotation,
            Transform instantiationParent, ICoordsUtils coordsUtils, IMapCullingController cullingController)
            : base(instantiationParent, coordsUtils, cullingController)
        {
            this.builder = builder;
            this.playerWorldPosition = playerWorldPosition;
            this.playerRotation = playerRotation;
        }

        public void Initialize()
        {
            playerMarker = builder(instantiationParent);
        }

        public UniTask Enable(CancellationToken cancellationToken)
        {
            playerMarker.SetActive(true);

            SetRotation();
            SetPosition();

            playerWorldPosition.OnChange += OnPlayerWorldPositionChange;
            playerRotation.OnChange += OnPlayerRotationChange;
            return UniTask.CompletedTask;
        }

        public UniTask Disable(CancellationToken cancellationToken)
        {
            playerMarker.SetActive(false);

            playerWorldPosition.OnChange -= OnPlayerWorldPositionChange;
            playerRotation.OnChange -= OnPlayerRotationChange;
            return UniTask.CompletedTask;
        }

        private void SetPosition()
        {
            var gridPosition = Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get());
            playerMarker.SetPosition(coordsUtils.PivotPosition(playerMarker, coordsUtils.CoordsToPositionWithOffset(gridPosition)));
        }

        private void OnPlayerRotationChange(Vector3 current, Vector3 previous)
        {
            SetRotation();
        }

        private void SetRotation()
        {
            var playerRot = playerRotation.Get();
            var markerRot = Quaternion.Euler(0, 0, Mathf.Atan2(-playerRot.x, playerRot.z) * Mathf.Rad2Deg);
            playerMarker.SetRotation(markerRot);
        }

        private void OnPlayerWorldPositionChange(Vector3 current, Vector3 previous)
        {
            SetPosition();
        }

        protected override void DisposeImpl()
        {
            playerMarker?.Dispose();
        }
    }
}
