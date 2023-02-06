using GPUSkinning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUSkinningThrottlerService : IGPUSkinningThrottlerService
{
    private Dictionary<IGPUSkinning, int> gpuSkinnings = new Dictionary<IGPUSkinning, int>();

    private Coroutine updateCoroutine;


    public void Initialize()
    {
        updateCoroutine = CoroutineStarter.Start(ThrottleUpdate());
    }

    public void Register(IGPUSkinning gpuSkinning, int framesBetweenUpdates)
    {
        gpuSkinnings.Add(gpuSkinning, framesBetweenUpdates);
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


    private IEnumerator ThrottleUpdate()
    {
        while (true)
        {
            foreach (KeyValuePair<IGPUSkinning, int> entry in gpuSkinnings)
            {
                if (Time.frameCount % entry.Value == 0)
                    entry.Key.Update();
            }
            yield return null;
        }
    }
}
