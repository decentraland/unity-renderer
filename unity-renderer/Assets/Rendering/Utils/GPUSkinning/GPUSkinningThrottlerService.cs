using Cysharp.Threading.Tasks;
using GPUSkinning;
using System.Collections.Generic;
using UnityEngine;

public class GPUSkinningThrottlerService : IGPUSkinningThrottlerService
{
    private readonly Dictionary<IGPUSkinning, int> gpuSkinnings = new ();

    private bool stopAsked;

    public void Initialize()
    {
        if (stopAsked)
        {
            Debug.LogError("GPUSkinningThrottlerService: Initialize called while stop was asked");
            return;
        }

        ThrottleUpdateAsync().Forget();
    }

    public void Register(IGPUSkinning gpuSkinning, int framesBetweenUpdates = 1)
    {
        if (!gpuSkinnings.ContainsKey(gpuSkinning))
            gpuSkinnings.Add(gpuSkinning, framesBetweenUpdates);
        else
            Debug.LogError("GPUSkinningThrottlerService: Register called twice for the same IGPUSkinning");
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
        stopAsked = true;
    }

    public void Dispose()
    {
        gpuSkinnings.Clear();
    }

    public static GPUSkinningThrottlerService Create(bool initializeOnSpawn)
    {
        GPUSkinningThrottlerService service = new GPUSkinningThrottlerService();

        if (initializeOnSpawn)
            service.Initialize();

        return service;
    }

    private async UniTaskVoid ThrottleUpdateAsync()
    {
        await UniTask.DelayFrame(1, PlayerLoopTiming.PostLateUpdate);

        while (!stopAsked)
        {
            foreach (KeyValuePair<IGPUSkinning, int> entry in gpuSkinnings)
            {
                if (Time.frameCount % entry.Value == 0)
                    entry.Key.Update();
            }

            await UniTask.DelayFrame(1, PlayerLoopTiming.PostLateUpdate);
        }

        stopAsked = false;
    }
}
