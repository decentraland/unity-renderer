using System;
using System.Collections.Generic;
using UnityEngine;

public class GPUSkinningComponent : MonoBehaviour
{
    public Material gpuSkinningMat;
    private List<SimpleGPUSkinning> gpuSkinnedRenderers = new List<SimpleGPUSkinning>();

    void Start()
    {
        var renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach ( var r in renderers )
        {
            var newSkinning = new SimpleGPUSkinning( r, gpuSkinningMat );
            gpuSkinnedRenderers.Add(newSkinning);
        }
    }

    void Update()
    {
        for (int i = 0; i < gpuSkinnedRenderers.Count; i++)
        {
            gpuSkinnedRenderers[i].Update();
        }
    }
}