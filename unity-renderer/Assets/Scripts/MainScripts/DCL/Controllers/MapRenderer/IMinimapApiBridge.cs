using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace DCL.Map
{
    public interface IMinimapApiBridge
    {
        UniTask<MinimapMetadata.MinimapSceneInfo[]> GetScenesInformationAroundParcel(Vector2Int coordinate,
            int areaSize,
            CancellationToken cancellationToken);
    }
}
