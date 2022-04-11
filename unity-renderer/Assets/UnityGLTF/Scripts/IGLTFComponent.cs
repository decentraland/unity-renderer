using System;
using DCL;
using DCL.Components;
using UnityEngine;

namespace UnityGLTF.Scripts
{
    public interface IGLTFComponent : ILoadable, IDownloadQueueElement
    {
        void Initialize(IWebRequestController webRequestController, IThrottlingCounter throttlingCounter);
        void LoadAsset(string assetDirectoryPath, string fileName, string id, bool loadEvenIfAlreadyLoaded, GLTFComponent.Settings settings, AssetIdConverter fileToHash);
        void RegisterCallbacks(Action<Mesh> meshCreated, Action<Renderer> rendererCreated);
        void SetPrioritized();
        long GetAnimationClipMemorySize();
        long GetMeshesMemorySize();
    }
}