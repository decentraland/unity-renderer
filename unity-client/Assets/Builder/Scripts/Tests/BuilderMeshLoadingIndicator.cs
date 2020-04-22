using DCL.Components;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BuilderMeshLoadingIndicator : TestsBase
{
    [UnityTest]
    public IEnumerator BuilderMeshLoadingIndicatorTest()
    {
        yield return InitScene();
        DCL.Configuration.EnvironmentSettings.DEBUG = true;
        sceneController.SetDebug();

        yield return SceneManager.LoadSceneAsync("BuilderScene", LoadSceneMode.Additive);

        var builderBridge = Object.FindObjectOfType<Builder.DCLBuilderBridge>();
        builderBridge.ResetBuilderScene();

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
