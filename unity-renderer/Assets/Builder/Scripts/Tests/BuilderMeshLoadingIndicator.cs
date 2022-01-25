using DCL.Components;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BuilderMeshLoadingIndicator : IntegrationTestSuite_Legacy
{
    private ParcelScene scene;

    [UnityTest]
    public IEnumerator BuilderMeshLoadingIndicatorTest()
    {
        yield return SceneManager.LoadSceneAsync("BuilderScene", LoadSceneMode.Additive);

        scene = TestUtils.CreateTestScene();

        var builderBridge = Object.FindObjectOfType<Builder.DCLBuilderBridge>();
        builderBridge.ResetBuilderScene();

        var objectEntity = TestUtils.CreateSceneEntity(scene);
        var objectShape = TestUtils.AttachGLTFShape(objectEntity, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
        });

        CheckActiveIndicatorsAmount(expectedAmount: 1);

        yield return TestUtils.WaitForGLTFLoad(objectEntity);

        CheckActiveIndicatorsAmount(expectedAmount: 0);

        yield return SceneManager.UnloadSceneAsync("BuilderScene");
    }

    void CheckActiveIndicatorsAmount(int expectedAmount)
    {
        var indicators = Object.FindObjectsOfType<Builder.MeshLoadIndicator.DCLBuilderMeshLoadIndicator>();

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