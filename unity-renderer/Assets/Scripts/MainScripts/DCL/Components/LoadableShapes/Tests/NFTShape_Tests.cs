using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using DCL;
using DCL.Controllers;
using DCL.World.PortableExperiences;
using NSubstitute;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class NFTShape_Tests : IntegrationTestSuite
{
    private ParcelScene scene;

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<ISceneController>(() => new SceneController(Substitute.For<IConfirmedExperiencesRepository>()));
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        scene = TestUtils.CreateTestScene();
        scene.contentProvider = new ContentProvider_Dummy();
        CommonScriptableObjects.rendererState.Set(true);
        TestUtils_NFT.RegisterMockedNFTShape(Environment.i.world.componentFactory);
    }

    [UnityTest]
    public IEnumerator ShapeUpdate()
    {
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);

        var entity = scene.entities[entityId];
        Assert.IsTrue(entity.meshRootGameObject == null, "entity mesh object should be null as the NFTShape hasn't been initialized yet");

        var componentModel = new NFTShape.Model()
        {
            src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
        };

        NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
        yield return component.routine;

        TestUtils.SharedComponentAttach(component, entity);

        Assert.IsTrue(entity.meshRootGameObject != null, "entity mesh object should already exist as the NFTShape already initialized");

        var nftShape = Environment.i.world.state.GetLoaderForEntity(entity) as LoadWrapper_NFT;

        var backgroundMaterial = nftShape.loaderController.backgroundMaterial;

        Assert.IsTrue(backgroundMaterial.GetColor("_BaseColor") == new Color(0.6404918f, 0.611472f, 0.8584906f), "The NFT frame background color should be the default one");

        // Update color and check if it changed
        componentModel.color = Color.yellow;
        yield return TestUtils.SharedComponentUpdate(component, componentModel);

        Assert.AreEqual(Color.yellow, backgroundMaterial.GetColor("_BaseColor"), "The NFT frame background color should be yellow");
    }

    [UnityTest]
    public IEnumerator MissingValuesGetDefaultedOnUpdate()
    {
        var component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE);
        yield return component.routine;

        Assert.IsFalse(component == null);

        yield return TestUtils.TestSharedComponentDefaultsOnUpdate<NFTShape.Model, NFTShape>(scene, CLASS_ID.NFT_SHAPE);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CollisionProperty()
    {
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape.Model();
        shapeModel.src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536";

        var shapeComponent = TestUtils.SharedComponentCreate<LoadableShape<LoadWrapper_NFT, NFTShape.Model>, LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model>(scene, CLASS_ID.NFT_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
        yield return new DCL.WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestUtils.TestShapeCollision(shapeComponent, shapeModel, entity);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator VisibleProperty()
    {
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape.Model();
        shapeModel.src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536";

        var shapeComponent = TestUtils.SharedComponentCreate<LoadableShape<LoadWrapper_NFT, NFTShape.Model>, LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model>(scene, CLASS_ID.NFT_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
        yield return new DCL.WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestUtils.TestOnPointerEventWithShapeVisibleProperty(shapeComponent, shapeModel, entity);
    }
}
