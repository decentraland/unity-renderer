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

        private AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>> property;

        public void Dispose() { }

        public void Initialize()
        {
            property = new AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>>(sceneInfos);
        }

        public IReadOnlyAsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>> ScenesInfo => property;

        public void EmitScenesInfo()
        {
            property.Value = sceneInfos;
        }

        public void SetUpdateMode(IHotScenesFetcher.UpdateMode mode) { }
    }
}
