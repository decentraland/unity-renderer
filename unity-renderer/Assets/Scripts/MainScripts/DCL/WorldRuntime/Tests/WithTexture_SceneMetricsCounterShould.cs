using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithTexture_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    public IEnumerator NotCountIdenticalTexturesWhenManyTextureComponentsAreAdded()
    {
        var texture1 = CreateTexture(texturePaths[0]);
        var texture2 = CreateTexture(texturePaths[0]);
        var texture3 = CreateTexture(texturePaths[0]);
        var texture4 = CreateTexture(texturePaths[0]);
        var material1 = CreatePBRMaterial(texture1.id, texture2.id, texture3.id, texture4.id);
        var planeShape = CreatePlane();
        var entity = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(planeShape, entity);
        TestUtils.SharedComponentAttach(material1, entity);
        TestUtils.SharedComponentAttach(texture1, entity);
        TestUtils.SharedComponentAttach(texture2, entity);
        TestUtils.SharedComponentAttach(texture3, entity);
        TestUtils.SharedComponentAttach(texture4, entity);

        yield return material1.routine;

        Assert.That( scene.metricsCounter.currentCount.textures, Is.EqualTo(1) );

        texture1.Dispose();
        texture2.Dispose();
        texture3.Dispose();
        texture4.Dispose();
        material1.Dispose();
        planeShape.Dispose();

        yield return null;

        Assert.That( scene.metricsCounter.currentCount.textures, Is.EqualTo(0) );
    }


    [UnityTest]
    public IEnumerator CountSingleTextureAttachedToManyBasicMaterials()
    {
        var texture = CreateTexture(texturePaths[0]);

        List<BasicMaterial> materials = new List<BasicMaterial>();
        List<IDCLEntity> entities = new List<IDCLEntity>();

        PlaneShape planeShape = CreatePlane();

        yield return planeShape.routine;
        yield return texture.routine;

        for ( int i = 0; i < 10; i++ )
        {
            materials.Add( CreateBasicMaterial(texture.id) );
            entities.Add( CreateEntityWithTransform());
            TestUtils.SharedComponentAttach(planeShape, entities[i]);
            TestUtils.SharedComponentAttach(materials[i], entities[i]);
            TestUtils.SharedComponentAttach(texture, entities[i]);
            yield return materials[i].routine;
        }

        SceneMetricsModel inputModel = scene.metricsCounter.currentCount;

        Assert.That( inputModel.materials, Is.EqualTo(10) );
        Assert.That( inputModel.textures, Is.EqualTo(1) );

        for ( int i = 0; i < 10; i++ )
        {
            materials[i].Dispose();
        }

        texture.Dispose();
        planeShape.Dispose();

        inputModel = scene.metricsCounter.currentCount;

        Assert.That( inputModel.materials, Is.EqualTo(0) );
        Assert.That( inputModel.textures, Is.EqualTo(0) );
    }

    [UnityTest]
    public IEnumerator CountSingleTextureWhenTextureIsSwappedOnSingleBasicMaterial()
    {
        var material = CreateBasicMaterial("");

        List<DCLTexture> textures = new List<DCLTexture>();
        SceneMetricsModel sceneMetrics;

        for ( int i = 0; i < 10; i++ )
        {
            textures.Add( CreateTexture(texturePaths[0]));
            yield return textures[i].routine;
        }

        IDCLEntity entity = CreateEntityWithTransform();
        TestUtils.SharedComponentAttach(material, entity);

        PlaneShape planeShape = CreatePlane();
        TestUtils.SharedComponentAttach(planeShape, entity);

        for ( int i = 0; i < 10; i++ )
        {
            TestUtils.SharedComponentAttach(textures[i], entity);

            yield return TestUtils.SharedComponentUpdate(material, new BasicMaterial.Model()
            {
                texture = textures[i].id
            });

            yield return null;

            sceneMetrics = scene.metricsCounter.currentCount;

            Assert.That( sceneMetrics.materials, Is.EqualTo(1) );
            Assert.That( sceneMetrics.textures, Is.EqualTo(1) );
        }

        foreach ( var texture in textures )
        {
            texture.Dispose();
        }

        material.Dispose();

        yield return null;

        sceneMetrics = scene.metricsCounter.currentCount;

        Assert.That( sceneMetrics.materials, Is.EqualTo(0) );
        Assert.That( sceneMetrics.textures, Is.EqualTo(0) );
    }


    [UnityTest]
    public IEnumerator CountManyAttachedTexturesToSinglePBRMaterial()
    {
        SceneMetricsModel sceneMetrics = null;

        var texture1 = CreateTexture(texturePaths[0]);
        var texture2 = CreateTexture(texturePaths[1]);
        var texture3 = CreateTexture(texturePaths[2]);
        var texture4 = CreateTexture(texturePaths[3]);
        var planeShape = CreatePlane();
        var entity = CreateEntityWithTransform();
        var material1 = CreatePBRMaterial(texture1.id, texture2.id, texture3.id, texture4.id);

        TestUtils.SharedComponentAttach(planeShape, entity);
        TestUtils.SharedComponentAttach(material1, entity);
        TestUtils.SharedComponentAttach(texture1, entity);
        TestUtils.SharedComponentAttach(texture2, entity);
        TestUtils.SharedComponentAttach(texture3, entity);
        TestUtils.SharedComponentAttach(texture4, entity);

        yield return material1.routine;

        sceneMetrics = scene.metricsCounter.currentCount;

        Assert.That( sceneMetrics.materials, Is.EqualTo(1) );
        Assert.That( sceneMetrics.textures, Is.EqualTo(4) );

        texture1.Dispose();
        texture2.Dispose();
        texture3.Dispose();
        texture4.Dispose();
        material1.Dispose();
        planeShape.Dispose();

        yield return null;

        sceneMetrics = scene.metricsCounter.currentCount;

        Assert.That( sceneMetrics.materials, Is.EqualTo(0) );
        Assert.That( sceneMetrics.textures, Is.EqualTo(0) );
    }

    [UnityTest]
    public IEnumerator CountWhenAdded()
    {
        Assert.Fail();
        yield break;
    }

    [UnityTest]
    public IEnumerator CountWhenRemoved()
    {
        Assert.Fail();
        yield break;
    }
}