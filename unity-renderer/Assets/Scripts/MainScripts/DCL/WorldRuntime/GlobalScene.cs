using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Models;
using MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB;
using UnityEngine;

namespace DCL.Controllers
{
    public class GlobalScene : ParcelScene
    {
        private const string NEW_CDN_FF = "ab-new-cdn";

        [System.NonSerialized]
        public string iconUrl;

        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();

        protected override string prettyName => $"{sceneData.id} - {sceneData.sceneNumber}{(isPortableExperience ? " (PE)" : "")}";

        public override bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f)
        {
            return true;
        }

        public override bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0)
        {
            return true;
        }

        public override async UniTask SetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            this.sceneData = data;

            contentProvider = new ContentProvider
            {
                baseUrl = data.baseUrl,
                contents = data.contents,
                sceneCid = data.id,
            };

            contentProvider.BakeHashes();

            if (featureFlags.IsFeatureEnabled(NEW_CDN_FF))
            {
                var sceneAb = await FetchSceneAssetBundles(data.id, data.baseUrlBundles);

                if (sceneAb.IsSceneConverted())
                {
                    contentProvider.assetBundles = sceneAb.GetConvertedFiles();
                    contentProvider.assetBundlesBaseUrl = sceneAb.GetBaseUrl();
                }
            }

            Vector3 gridToWorldPosition = Utils.GridToWorldPosition(data.basePosition.x, data.basePosition.y);
            gameObject.transform.position = PositionUtils.WorldToUnityPosition(gridToWorldPosition);

            DataStore.i.sceneWorldObjects.AddScene(sceneData.sceneNumber);
        }

        private async UniTask<Asset_SceneAB> FetchSceneAssetBundles(string sceneId, string dataBaseUrlBundles)
        {
            AssetPromise_SceneAB promiseSceneAb = new AssetPromise_SceneAB(dataBaseUrlBundles, sceneId);
            AssetPromiseKeeper_SceneAB.i.Keep(promiseSceneAb);
            await promiseSceneAb.ToUniTask();
            return promiseSceneAb.asset;
        }

        protected override void SendMetricsEvent() { }
    }
}
