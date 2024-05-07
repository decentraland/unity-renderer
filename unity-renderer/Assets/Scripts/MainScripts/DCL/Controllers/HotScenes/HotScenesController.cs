using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Environment = DCL.Environment;

namespace MainScripts.DCL.Controllers.HotScenes
{
    /// <summary>
    /// We have to keep this class to communicate with Kernel
    /// </summary>
    public class HotScenesController : MonoBehaviour, IHotScenesController
    {
        [Obsolete]
        public static HotScenesController i { get; private set; }

        [Obsolete]
        public event Action OnHotSceneListFinishUpdating;

        [Obsolete]
        public event Action OnHotSceneListChunkUpdated;

        public List<IHotScenesController.HotSceneInfo> hotScenesList => hotScenes;

        [Obsolete]
        public float timeSinceLastUpdate => Time.realtimeSinceStartup - lastUpdateTime;

        [Obsolete]
        private float lastUpdateTime = float.MinValue * .5f;

        private List<IHotScenesController.HotSceneInfo> hotScenes = new ();

        [Serializable]
        internal struct HotScenesUpdatePayload
        {
            public int chunkIndex;
            public int chunksCount;
            public IHotScenesController.HotSceneInfo[] scenesInfo;
        }

        private readonly AutoResetUniTaskCompletionSource<IReadOnlyList<IHotScenesController.HotSceneInfo>> source
            = AutoResetUniTaskCompletionSource<IReadOnlyList<IHotScenesController.HotSceneInfo>>.Create();

        private void Awake()
        {
            i = this;
        }

        public UniTask<IReadOnlyList<IHotScenesController.HotSceneInfo>> GetHotScenesListAsync(CancellationToken cancellationToken)
        {
            WebInterface.FetchHotScenes();
            return source.Task.AttachExternalCancellation(cancellationToken);
        }

        public async UniTask<IReadOnlyList<IHotScenesController.HotWorldInfo.WorldInfo>> GetHotWorldsListAsync(CancellationToken cancellationToken)
        {
            UnityWebRequest result = await Environment.i.platform.webRequest.GetAsync("https://worlds-content-server.decentraland.org/live-data", cancellationToken: cancellationToken);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching hot world info:\n{result.error}");

            var response = Utils.SafeFromJson<IHotScenesController.HotWorldInfo>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing hot world info:\n{result.downloadHandler.text}");

            if (response.data == null)
                throw new Exception($"No worlds info retrieved:\n{result.downloadHandler.text}");

            return response.data.perWorld != null ? response.data.perWorld : new List<IHotScenesController.HotWorldInfo.WorldInfo>();
        }

        [UsedImplicitly]
        public void UpdateHotScenesList(string json)
        {
            var updatePayload = Utils.SafeFromJson<HotScenesUpdatePayload>(json);

            if (updatePayload.chunkIndex == 0) { hotScenes.Clear(); }

            hotScenes.AddRange(updatePayload.scenesInfo);
            OnHotSceneListChunkUpdated?.Invoke();

            if (updatePayload.chunkIndex >= updatePayload.chunksCount - 1)
            {
                lastUpdateTime = Time.realtimeSinceStartup;
                OnHotSceneListFinishUpdating?.Invoke();
                source.TrySetResult(hotScenesList);
            }
        }

        public void Dispose()
        {
            hotScenes.Clear();
        }

        public void Initialize() { }
    }
}
