using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DCL.Helpers;
using DCL.Components;
using NUnit.Framework;

namespace Builder.Tests
{
    public class BuilderMeshLoadingIndicator : TestsBase
    {
        [UnityTest]
        public IEnumerator BuilderMeshLoadingIndicatorTest()
        {
            yield return InitScene();
            DCL.Configuration.EnvironmentSettings.DEBUG = true;
            sceneController.SetDebug();
            yield return new WaitForSeconds(0.1f);

            sceneController.UnloadAllScenes();
            var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;
            sceneController.LoadParcelScenes(scenesToLoad);

            yield return new WaitForAllMessagesProcessed();
            yield return SceneManager.LoadSceneAsync("BuilderScene", LoadSceneMode.Additive);

            var builderBridge = Object.FindObjectOfType<Builder.DCLBuilderBridge>();
            builderBridge.ResetBuilderScene();

            var scene = sceneController.loadedScenes["0,0"];

            var objectEntity = TestHelpers.CreateSceneEntity(scene);
            var objectShape = TestHelpers.AttachGLTFShape(objectEntity, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
            {
                src = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            });

            CheckActiveIndicatorsAmount(expectedAmount: 1);

            yield return TestHelpers.WaitForGLTFLoad(objectEntity);

            CheckActiveIndicatorsAmount(expectedAmount: 0);

            sceneController.UnloadAllScenes();
            yield return null;
        }

        void CheckActiveIndicatorsAmount(int expectedAmount)
        {
            Builder.MeshLoadIndicator.DCLBuilderMeshLoadIndicator[] indicators = Object.FindObjectsOfType<Builder.MeshLoadIndicator.DCLBuilderMeshLoadIndicator>();
            int activeIndicators = 0;
            for (int i = 0; i < indicators.Length; i++)
            {
                if (indicators[i].gameObject.activeInHierarchy)
                {
                    activeIndicators++;
                }
            }

            Assert.AreEqual(expectedAmount, activeIndicators, $"Expected {expectedAmount} active loading indicator");
        }
    }
}