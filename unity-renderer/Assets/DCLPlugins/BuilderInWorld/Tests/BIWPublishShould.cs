using System.Collections;
using DCL;
using DCL.Builder;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
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
    private CoreComponentsPlugin coreComponentsPlugin;

    private const long entityId = 1;

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

        coreComponentsPlugin = new CoreComponentsPlugin();
        BuilderInWorldPlugin.RegisterRuntimeComponents();
        scene = TestUtils.CreateTestScene();
        
        var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);
        biwPublishController.Initialize(context);
        biwEntityHandler.Initialize(context);

        biwPublishController.EnterEditMode(builderScene);
        biwEntityHandler.EnterEditMode(builderScene);
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

        LoadWrapper gltfShape =
            Environment.i.world.state.GetLoaderForEntity(scene.entities[entity.rootEntity.entityId]);
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
        BuilderInWorldPlugin.UnregisterRuntimeComponents();
        coreComponentsPlugin.Dispose();
        Object.Destroy(biwBridge.gameObject);
        biwPublishController.Dispose();
        biwEntityHandler.Dispose();
        yield return base.TearDown();
    }
}