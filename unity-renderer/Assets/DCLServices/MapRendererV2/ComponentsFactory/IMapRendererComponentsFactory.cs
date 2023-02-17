using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.MapLayers;
using System.Threading;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    /// <summary>
    /// Abstraction needed to produce different types of elements and resolve dependencies between them
    /// based on the type of `MapRenderer`: e.g. Chunk based vs Shader based
    /// </summary>
    public interface IMapRendererComponentsFactory
    {
        internal UniTask<MapRendererComponents> Create(CancellationToken cancellationToken);
    }
}
