using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    public class MapRendererChunkComponentsFactory : IMapRendererComponentsFactory
    {
        private const string ATLAS_CHUNK_ADDRESS = "AtlasChunk";
        private const string MAP_CONFIGURATION_ADDRESS = "MapRendererConfiguration";

        private const int ATLAS_DRAW_ORDER = 1;
        private const int ATLAS_CHUNK_SIZE = 250;

        private const int PARCEL_SIZE = 20;

        private Service<IAddressableResourceProvider> addressablesProvider;

        private IAddressableResourceProvider AddressableProvider => addressablesProvider.Ref;

        MapRendererComponents IMapRendererComponentsFactory.Create(CancellationToken cancellationToken)
        {
            // TODO implement Culling Controller
            IMapCullingController cullingController = null;

            var enumerator = UniTaskAsyncEnumerable.Create<(MapLayer, IMapLayerController)>(async (writer, token) =>
            {
                var configuration = await AddressableProvider.GetAddressable<MapRendererConfiguration>(MAP_CONFIGURATION_ADDRESS, cancellationToken);
                var coordsUtils = new ChunkCoordsUtils(PARCEL_SIZE);

                async UniTask CreateAtlas()
                {
                    var atlasChunkPrefab = await AddressableProvider.GetAddressable<SpriteRenderer>(ATLAS_CHUNK_ADDRESS, cancellationToken);
                    var chunkAtlas = new ChunkAtlasController(configuration.AtlasRoot, atlasChunkPrefab, ATLAS_DRAW_ORDER, ATLAS_CHUNK_SIZE,
                        coordsUtils, cullingController, ChunkController.CreateChunk);

                    // initialize Atlas but don't block the flow (to accelerate loading time)
                    chunkAtlas.Initialize(cancellationToken).SuppressCancellationThrow().Forget();

                    await writer.YieldAsync((MapLayer.Atlas, chunkAtlas));
                }

                await UniTask.WhenAll(CreateAtlas() /* List of other creators that can be executed in parallel */);
            });

            return new MapRendererComponents(enumerator, cullingController);
        }
    }
}
