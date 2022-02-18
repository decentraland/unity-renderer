using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = UnityEngine.WaitUntil;

public class BIWPublishShould : IntegrationTestSuite_Legacy
{
    private BIWPublishController biwPublishController;
    private BIWEntityHandler biwEntityHandler;
    private BuilderInWorldBridge biwBridge;
    private IContext context;
    private ParcelScene scene;

    private const string entityId = "E1";

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        biwPublishController = new BIWPublishController();
        biwEntityHandler = new BIWEntityHandler();
        biwBridge = MainSceneFactory.CreateBuilderInWorldBridge();

        context = BIWTestUtils.CreateContextWithGenericMocks(
            biwPublishController,
            biwEntityHandler
        );

        biwPublishController.Initialize(context);
        biwEntityHandler.Initialize(context);

        scene = TestUtils.CreateTestScene();

        biwPublishController.EnterEditMode(scene);
        biwEntityHandler.EnterEditMode(scene);
    }

    [Test]
    public void TestEntityOutsidePublish()
    {
        //Arrange
        BIWEntity entity = biwEntityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);

        //Act
        entity.gameObject.transform.position = Vector3.one * 9999;

        //Assert
        Assert.IsFalse(biwPublishController.CanPublish());
    }

    [UnityTest]
    public IEnumerator TestEntityInsidePublish()
    {
        //Arrange
        BIWEntity entity = biwEntityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);
        TestUtils.CreateAndSetShape(scene, entity.rootEntity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entity.rootEntity.entityId]);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        //Act
        entity.rootEntity.gameObject.transform.position = new Vector3(5, 0, 5);

        //Assert
        Assert.IsTrue(biwPublishController.CanPublish());
    }

    [Test]
    public void TestMetricsPublish()
    {
        //Act
        for (int i = 0; i < scene.metricsCounter.maxCount.entities + 1; i++)
        {
            TestUtils.CreateSceneEntity(scene, entityId + i);
        }

        //Assert
        Assert.IsFalse(biwPublishController.CanPublish());
    }

    [Test]
    public void TestPublishFeedbackMessage()
    {
        //Act
        string result = biwPublishController.CheckPublishConditions();

        //Assert
        Assert.AreEqual(result, "");
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(biwBridge.gameObject);
        biwPublishController.Dispose();
        biwEntityHandler.Dispose();
        yield return base.TearDown();
    }
}