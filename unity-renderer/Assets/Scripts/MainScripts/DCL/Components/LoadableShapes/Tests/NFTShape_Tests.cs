using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class NFTShape_Tests : TestsBase
{

    [UnityTest]
    public IEnumerator ShapeUpdate()
    {


        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        var entity = scene.entities[entityId];
        Assert.IsTrue(entity.meshRootGameObject == null, "entity mesh object should be null as the NFTShape hasn't been initialized yet");

        var componentModel = new NFTShape.Model()
        {
            src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
        };

        NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
        yield return component.routine;

        TestHelpers.SharedComponentAttach(component, entity);

        Assert.IsTrue(entity.meshRootGameObject != null, "entity mesh object should already exist as the NFTShape already initialized");

        var nftShape = LoadableShape.GetLoaderForEntity(entity) as LoadWrapper_NFT;

        var backgroundMaterial = nftShape.loaderController.meshRenderer.materials[1];

        Assert.IsTrue(backgroundMaterial.GetColor("_BaseColor") == new Color(0.6404918f, 0.611472f, 0.8584906f), "The NFT frame background color should be the default one");

        // Update color and check if it changed
        componentModel.color = Color.yellow;
        yield return TestHelpers.SharedComponentUpdate(component, componentModel);

        Assert.AreEqual(Color.yellow, backgroundMaterial.GetColor("_BaseColor"), "The NFT frame background color should be yellow");
    }

    [UnityTest]
    public IEnumerator MissingValuesGetDefaultedOnUpdate()
    {


        var component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE);
        yield return component.routine;

        Assert.IsFalse(component == null);

        yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<NFTShape.Model, NFTShape>(scene, CLASS_ID.NFT_SHAPE);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CollisionProperty()
    {


        string entityId = "entityId";
        TestHelpers.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model();
        shapeModel.src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536";

        var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_NFT, NFTShape.Model>, LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model>(scene, CLASS_ID.NFT_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestHelpers.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
        yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator VisibleProperty()
    {
        string entityId = "entityId";
        TestHelpers.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model();
        shapeModel.src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536";

        var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_NFT, NFTShape.Model>, LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model>(scene, CLASS_ID.NFT_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestHelpers.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
        yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);
    }
}
