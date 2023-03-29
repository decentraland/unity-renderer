using Cysharp.Threading.Tasks;
using GPUSkinning;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GPUSkinningThrottlerService : IGPUSkinningThrottlerService
{
    private readonly Dictionary<IGPUSkinning, int> gpuSkinnings = new ();

    private CancellationTokenSource cts;

    public void Initialize()
    {
        cts = new CancellationTokenSource();
        ThrottleUpdateAsync(cts.Token).Forget();
    }

    public void Register(IGPUSkinning gpuSkinning, int framesBetweenUpdates = 1)
    {
        if (!gpuSkinnings.ContainsKey(gpuSkinning))
            gpuSkinnings.Add(gpuSkinning, framesBetweenUpdates);
        else
            Debug.LogWarning($"GPUSkinningThrottlerService: Register called twice for the same IGPUSkinning {gpuSkinning}", gpuSkinning.renderer.gameObject);
    }

    public void Unregister(IGPUSkinning gpuSkinning)
    {
        if (gpuSkinnings.ContainsKey(gpuSkinning))
            gpuSkinnings.Remove(gpuSkinning);
    }

    public void ModifyThrottling(IGPUSkinning gpuSkinning, int framesBetweenUpdates)
    {
        if (gpuSkinnings.ContainsKey(gpuSkinning))
            gpuSkinnings[gpuSkinning] = framesBetweenUpdates;
    }

    public void ForceStop()
    {
        Cancel();
    }

    public void Dispose()
    {
        Cancel();
        gpuSkinnings.Clear();
    }

    public static GPUSkinningThrottlerService Create(bool initializeOnSpawn)
    {
        GPUSkinningThrottlerService service = new GPUSkinningThrottlerService();

        if (initializeOnSpawn)
            service.Initialize();

        return service;
    }

    private void Cancel()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    private async UniTaskVoid ThrottleUpdateAsync(CancellationToken ct)
    {
        await UniTask.DelayFrame(1, PlayerLoopTiming.PostLateUpdate);

        // Cancel gracefully
        while (!ct.IsCancellationRequested)
        {
            foreach (KeyValuePair<IGPUSkinning, int> entry in gpuSkinnings)
            {
                if (Time.frameCount % entry.Value == 0)
                    entry.Key.Update();
            }

            await UniTask.DelayFrame(1, PlayerLoopTiming.PostLateUpdate);
        }
    }
}
