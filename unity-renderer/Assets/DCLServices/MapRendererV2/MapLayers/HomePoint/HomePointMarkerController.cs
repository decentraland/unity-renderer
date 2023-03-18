using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.HomePoint
{
    internal class HomePointMarkerController : MapLayerControllerBase, IMapLayerController
    {
        internal delegate IHomePointMarker HomePointMarkerBuilder(Transform parent);

        private readonly BaseVariable<Vector2Int> homePointCoordinates;
        private readonly HomePointMarkerBuilder builder;

        private IHomePointMarker marker;

        public HomePointMarkerController(
            BaseVariable<Vector2Int> homePointCoordinates,
            HomePointMarkerBuilder builder,
            Transform instantiationParent,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController)
            : base(instantiationParent, coordsUtils, cullingController)
        {
            this.homePointCoordinates = homePointCoordinates;
            this.builder = builder;
        }

        public void Initialize()
        {
            marker = builder(instantiationParent);
            marker.SetActive(false);
        }

        private void SetMarkerPosition(Vector2Int position)
        {
            marker.SetPosition(coordsUtils.CoordsToPosition(position));
        }

        public UniTask Enable(CancellationToken cancellationToken)
        {
            marker.SetActive(true);
            SetMarkerPosition(homePointCoordinates.Get());

            homePointCoordinates.OnChange += OnHomePointCoordinatesChange;

            return UniTask.CompletedTask;
        }

        public UniTask Disable(CancellationToken cancellationToken)
        {
            marker.SetActive(false);

            homePointCoordinates.OnChange -= OnHomePointCoordinatesChange;

            return UniTask.CompletedTask;
        }

        private void OnHomePointCoordinatesChange(Vector2Int current, Vector2Int previous)
        {
            SetMarkerPosition(current);
        }
    }
}
