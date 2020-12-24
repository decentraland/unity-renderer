using DCL.Components;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BuilderMeshLoadingIndicator : TestsBase
{
    protected override bool justSceneSetUp => true;

    [UnityTest]
    public IEnumerator BuilderMeshLoadingIndicatorTest()
    {
        SetUp_SceneController();
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