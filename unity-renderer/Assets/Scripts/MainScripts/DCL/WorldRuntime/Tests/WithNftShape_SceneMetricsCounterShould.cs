using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WithNftShape_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    [Explicit("Will be implemented later")]
    [Category("Explicit")]
    public IEnumerator CountWhenAddedWithGif()
    {
        Assert.Fail();
        yield break;
    }

    [UnityTest]
    [Explicit("Will be implemented later")]
    [Category("Explicit")]
    public IEnumerator CountWhenRemovedWithGif()
    {
        Assert.Fail();
        yield break;
    }

    [UnityTest]
    [Explicit("Will be implemented later")]
    [Category("Explicit")]
    public IEnumerator CountWhenRemovedWithImages()
    {
        Assert.Fail();
        yield break;
    }

    [UnityTest]
    [Explicit("Will be implemented later")]
    [Category("Explicit")]
    public IEnumerator CountWhenAddedWithImages()
    {
        var entity = TestUtils.CreateSceneEntity(scene);

        Assert.IsTrue(entity.meshRootGameObject == null, "entity mesh object should be null as the NFTShape hasn't been initialized yet");

        var componentModel = new NFTShape.Model()
        {
            src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
        };

        NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
        Debug.Log(scene.metricsCounter.currentCount);
        TestUtils.SharedComponentAttach(component, entity);

        LoadWrapper_NFT wrapper = Environment.i.world.state.GetLoaderForEntity(entity) as LoadWrapper_NFT;
        yield return new WaitUntil(() => wrapper.alreadyLoaded);

        Debug.Log(scene.metricsCounter.currentCount);
        AssertMetricsModel(scene,
            triangles: 190,
            materials: 6,
            entities: 1,
            meshes: 4,
            bodies: 4,
            textures: 0);

        component.Dispose();

        yield return null;

        AssertMetricsModel(scene,
            triangles: 0,
            materials: 0,
            entities: 0,
            meshes: 0,
            bodies: 0,
            textures: 0);
    }
}