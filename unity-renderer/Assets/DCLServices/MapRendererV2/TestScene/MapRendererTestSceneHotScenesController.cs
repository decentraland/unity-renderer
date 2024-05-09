using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestSceneHotScenesController : MonoBehaviour, IHotScenesFetcher
    {
        [SerializeField] private IHotScenesController.HotSceneInfo[] sceneInfos = Array.Empty<IHotScenesController.HotSceneInfo>();
        [SerializeField] private IHotScenesController.HotWorldInfo.WorldInfo[] worldInfos = Array.Empty<IHotScenesController.HotWorldInfo.WorldInfo>();

        private AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>> property;
        private AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotWorldInfo.WorldInfo>> worldsProperty;

        public void Dispose() { }

        public void Initialize()
        {
            property = new AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>>(sceneInfos);
        }

        public IReadOnlyAsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>> ScenesInfo => property;
        public IReadOnlyAsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotWorldInfo.WorldInfo>> WorldsInfo => worldsProperty;

        public void EmitScenesInfo()
        {
            property.Value = sceneInfos;
            worldsProperty.Value = worldInfos;
        }

        public void SetUpdateMode(IHotScenesFetcher.UpdateMode mode) { }
    }
}
