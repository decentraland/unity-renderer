using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers
{
    internal abstract class MapLayerControllerBase
    {
        protected ICoordsUtils coordsUtils { get; }
        protected CancellationTokenSource ctsDisposing { get; }
        protected Transform instantiationParent { get; }
        protected IMapCullingController mapCullingController { get; }

        protected MapLayerControllerBase(Transform instantiationParent, ICoordsUtils coordsUtils, IMapCullingController cullingController)
        {
            ctsDisposing = new CancellationTokenSource();
            this.coordsUtils = coordsUtils;
            this.instantiationParent = instantiationParent;
            this.mapCullingController = cullingController;
        }

        public void Dispose()
        {
            ctsDisposing.Cancel();
            ctsDisposing.Dispose();
            DisposeImpl();
        }

        protected virtual void DisposeImpl() { }

        protected CancellationTokenSource LinkWithDisposeToken(CancellationToken globalCancellation) =>
            CancellationTokenSource.CreateLinkedTokenSource(globalCancellation, ctsDisposing.Token);
    }
}
