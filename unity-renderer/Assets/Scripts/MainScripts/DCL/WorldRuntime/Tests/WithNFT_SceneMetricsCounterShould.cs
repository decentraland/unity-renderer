using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WithNFT_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        TestUtils_NFT.RegisterMockedNFTShape(Environment.i.world.componentFactory);
    }
    

    [UnityTest]
    public IEnumerator CountSingleNFT()
    {
        var entity = TestUtils.CreateSceneEntity(scene);

        TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(0, 0, 0)});

        var componentModel = new NFTShape.Model()
        {
            src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
        };

        NFTShape component =
            TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
        yield return component.routine;

        TestUtils.SharedComponentAttach(component, entity);

        LoadWrapper shapeLoader = Environment.i.world.state.GetLoaderForEntity(entity);
        yield return new UnityEngine.WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return null;

        Assert.That(scene.metricsCounter.currentCount.textures, Is.EqualTo(3));
        Assert.That(scene.metricsCounter.currentCount.materials, Is.EqualTo(6));
        Assert.That(scene.metricsCounter.currentCount.bodies, Is.EqualTo(4));
        Assert.That(scene.metricsCounter.currentCount.meshes, Is.EqualTo(4));
    }
}