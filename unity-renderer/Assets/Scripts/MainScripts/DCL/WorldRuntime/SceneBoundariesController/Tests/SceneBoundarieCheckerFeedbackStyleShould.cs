using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class SceneBoundarieCheckerFeedbackStyleShould : IntegrationTestSuite
{
    protected override WorldRuntimeContext CreateRuntimeContext()
    {
        return DCL.Tests.WorldRuntimeContextFactory.CreateWithCustomMocks
        (
            sceneController: new SceneController(),
            state: new WorldState(),
            componentFactory: new RuntimeComponentFactory(),
            sceneBoundsChecker: new SceneBoundsChecker(new SceneBoundsFeedbackStyle_Simple())
        );
    }

    [Test]
    public void ChangeFeedbackStyleChange()
    {
        //Arrange
        var redFlickerStyle = new SceneBoundsFeedbackStyle_RedFlicker();

        //Act
        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(redFlickerStyle);

        //Assert
        Assert.AreSame( Environment.i.world.sceneBoundsChecker.GetFeedbackStyle(), redFlickerStyle );
    }

    [UnityTest]
    public IEnumerator RedFlickerStyleApply()
    {
        //Arrange
        ParcelScene scene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene();

        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple());
        var redFlickerStyle = new SceneBoundsFeedbackStyle_RedFlicker();
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

        yield return new DCL.WaitUntil(() => !scene.entities[entityId].meshesInfo.renderers[0].enabled);
        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(redFlickerStyle);

        yield return null;

        //Assert
        foreach (var renderer in scene.entities[entityId].renderers)
        {
            Assert.IsTrue(renderer.enabled);
        }
        Assert.Greater( redFlickerStyle.GetInvalidMeshesCount(), 0);
    }
}