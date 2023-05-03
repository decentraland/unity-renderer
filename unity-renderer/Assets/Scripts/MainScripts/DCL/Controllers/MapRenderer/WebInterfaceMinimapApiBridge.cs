using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using System;
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
            MinimapMetadataPayload payload = JsonUtility.FromJson<MinimapMetadataPayload>(scenesInfoJson);

            if(!string.IsNullOrEmpty(scenesInfoJson) && !payload.isWorldScene)
                foreach (var sceneInfo in payload.scenesInfo)
                    minimapMetadata.AddSceneInfo(sceneInfo);

            if (!pendingTasks.ContainsKey(GET_SCENES_INFO_ID)) return;
            var task = (UniTaskCompletionSource<MinimapMetadata.MinimapSceneInfo[]>) pendingTasks[GET_SCENES_INFO_ID];
            pendingTasks.Remove(GET_SCENES_INFO_ID);
            task.TrySetResult(payload.scenesInfo);
        }

        public UniTask<MinimapMetadata.MinimapSceneInfo[]> GetScenesInformationAroundParcel(Vector2Int coordinate, int areaSize, CancellationToken cancellationToken)
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

    [Serializable]
    public class MinimapMetadataPayload
    {
        public bool isWorldScene;
        public MinimapMetadata.MinimapSceneInfo[] scenesInfo;
    }
}
