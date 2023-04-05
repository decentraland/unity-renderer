using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCLServices.MapRendererV2.MapLayers
{
    internal interface IMapLayerController : IDisposable
    {
        /// <summary>
        /// Enable layer
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token is bound to both `Abort` (changing to the `Disabled` state) and `Dispose`</param>
        UniTask Enable(CancellationToken cancellationToken);

        /// <summary>
        /// Disable layer
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token is bound to both `Abort` (changing to the `Enabled` state) and `Dispose`</param>
        UniTask Disable(CancellationToken cancellationToken);
    }
}
