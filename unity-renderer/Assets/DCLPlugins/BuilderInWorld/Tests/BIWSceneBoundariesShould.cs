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

public class BIWSceneBoundariesShould : IntegrationTestSuite
{
    private CoreComponentsPlugin coreComponentsPlugin;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        coreComponentsPlugin = new CoreComponentsPlugin();
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        yield return base.TearDown();
    }
    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<ISceneController>(() => new SceneController());
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
        serviceLocator.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
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
        ParcelScene scene = TestUtils.CreateTestScene();

        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple());
        var biwStyle = new SceneBoundsFeedbackStyle_BIW();
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
            "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

        TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
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


}