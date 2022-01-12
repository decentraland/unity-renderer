using System;
using DCL;
using DCL.Components;
using MainScripts.DCL.GLTF;
using UnityEngine;

namespace UnityGLTF.Scripts
{
    public interface IGLTFComponent : ILoadable, IDownloadQueueElement
    {
        void Initialize(IWebRequestController webRequestController, GLTFThrottlingCounter iThrottlingCounter);
        void LoadAsset(string assetDirectoryPath, string fileName, string getId, bool b, GLTFComponent.Settings tmpSettings, AssetIdConverter fileToHash);
        void RegisterCallbacks(Action<Mesh> meshCreated, Action<Renderer> rendererCreated);
        void SetPrioritized();
    }
}