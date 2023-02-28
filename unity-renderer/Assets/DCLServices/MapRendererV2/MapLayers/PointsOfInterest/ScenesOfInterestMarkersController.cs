using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using System;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PointsOfInterest
{
    internal class ScenesOfInterestMarkersController : MapLayerControllerBase, IMapCullingListener<ISceneOfInterestMarker>, IMapLayerController
    {
        private readonly MinimapMetadata minimapMetadata;

        public ScenesOfInterestMarkersController(MinimapMetadata minimapMetadata, Transform instantiationParent, ICoordsUtils coordsUtils, IMapCullingController cullingController, int drawOrder)
            : base(instantiationParent, coordsUtils, cullingController, drawOrder)
        {
            this.minimapMetadata = minimapMetadata;
        }

        public UniTask Initialize()
        {
            // non-blocking retrieval of scenes of interest happens independently on the minimap rendering
            return UniTask.CompletedTask;
        }

        public void OnMapObjectBecameVisible(ISceneOfInterestMarker marker)
        {
            marker.OnBecameVisible();
        }

        public void OnMapObjectCulled(ISceneOfInterestMarker marker)
        {
            marker.OnBecameInvisible();
        }

        public UniTask Enable(CancellationToken cancellationToken)
        {

        }

        public UniTask Disable(CancellationToken cancellationToken)
        {

        }
    }
}
