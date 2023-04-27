using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Map
{
    public class WebInterfaceMinimapApiBridge : MonoBehaviour, IMinimapApiBridge
    {
        private const string GET_SCENES_INFO_ID = "GetScenesInformationAroundParcel";

        public static WebInterfaceMinimapApiBridge i { get; private set; }

        private readonly Dictionary<string, IUniTaskSource> pendingTasks = new ();
        private MinimapMetadata minimapMetadata => MinimapMetadata.GetMetadata();

        private void Awake()
        {
            i ??= this;
        }

        [PublicAPI]
        public void UpdateMinimapSceneInformation(string scenesInfoJson)
        {
            MinimapMetadata.MinimapSceneInfo[] scenesInfo = new MinimapMetadata.MinimapSceneInfo[] { };

            if(!string.IsNullOrEmpty(scenesInfoJson))
                scenesInfo = Utils.ParseJsonArray<MinimapMetadata.MinimapSceneInfo[]>(scenesInfoJson);

            foreach (var sceneInfo in scenesInfo)
                minimapMetadata.AddSceneInfo(sceneInfo);

            if (!pendingTasks.ContainsKey(GET_SCENES_INFO_ID)) return;
            var task = (UniTaskCompletionSource<MinimapMetadata.MinimapSceneInfo[]>) pendingTasks[GET_SCENES_INFO_ID];
            pendingTasks.Remove(GET_SCENES_INFO_ID);
            task.TrySetResult(scenesInfo);
        }

        public UniTask<MinimapMetadata.MinimapSceneInfo[]> GetScenesInformationAroundParcel(Vector2Int coordinate,
            int areaSize,
            CancellationToken cancellationToken)
        {
            if (pendingTasks.TryGetValue(GET_SCENES_INFO_ID, out var pendingTask))
                return ((UniTaskCompletionSource<MinimapMetadata.MinimapSceneInfo[]>) pendingTask)
                      .Task.AttachExternalCancellation(cancellationToken);

            UniTaskCompletionSource<MinimapMetadata.MinimapSceneInfo[]> task = new ();
            pendingTasks[GET_SCENES_INFO_ID] = task;

            WebInterface.RequestScenesInfoAroundParcel(coordinate, areaSize);

            return task.Task.AttachExternalCancellation(cancellationToken);
        }
    }
}
