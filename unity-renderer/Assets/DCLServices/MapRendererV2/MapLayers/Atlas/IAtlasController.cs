using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCLServices.MapRendererV2.MapLayers.Atlas
{
    internal interface IAtlasController : IMapLayerController
    {
        UniTask Initialize(CancellationToken ct);
    }
}
