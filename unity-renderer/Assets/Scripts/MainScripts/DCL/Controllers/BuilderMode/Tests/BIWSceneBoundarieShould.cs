using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using DCL.Controllers;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

public class BIWSceneBoundarieShould : IntegrationTestSuite
{
    protected override PlatformContext CreatePlatformContext()
    {
        WebRequestController webRequestController = new WebRequestController();
        webRequestController.Initialize(
            genericWebRequest: new WebRequest(),
            assetBundleWebRequest: new WebRequestAssetBundle(),
            textureWebRequest: new WebRequestTexture(),
            null);

        var context = DCL.Tests.PlatformContextFactory.CreateWithCustomMocks
        (
            webRequestController: webRequestController
        );

        return context;
    }

    protected override WorldRuntimeContext CreateRuntimeContext()
    {
        return DCL.Tests.WorldRuntimeContextFactory.CreateWithCustomMocks
        (
            sceneController: new SceneController(),
            state: new WorldState(),
            componentFactory: new RuntimeComponentFactory(),
            sceneBoundsChecker: new SceneBoundsChecker()
        );
    }

    [Test]
    public void BuilderInWorldChangeFeedbackStyleChange()
    {
        //Arrange
        var biwStyle = new SceneBoundsFeedbackStyle_BIW();

        //Act
        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(biwStyle);

        //Assert
        Assert.AreSame( Environment.i.world.sceneBoundsChecker.GetFeedbackStyle(), biwStyle );
    }

    [UnityTest]
    public IEnumerator BuilderInWorldRendererEnableOutsideParcel()
    {
        //Arrange
        WebRequestController.Create();
        ParcelScene scene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene();

        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple());
        var biwStyle = new SceneBoundsFeedbackStyle_BIW();
        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
            "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

        TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        //Act
        scene.entities[entityId].gameObject.transform.position = new Vector3(100, 100, 100);
        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(biwStyle);

        yield return null;

        //Assert
        foreach (var renderer in scene.entities[entityId].renderers)
        {
            Assert.IsTrue(renderer.enabled);
        }
    }

    protected override IEnumerator TearDown() { yield return base.TearDown(); }
}