using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithBasicMaterial_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    public IEnumerator NotCountBasicMaterialsWhenNoShapeIsPresent()
    {
        BasicMaterial material1 = CreateBasicMaterial("");

        yield return material1.routine;

        Assert.That( scene.metricsCounter.currentCount.materials, Is.EqualTo(0) );
    }


    [UnityTest]
    public IEnumerator NotCountWhenAttachedToIgnoredEntities()
    {
        IDCLEntity entity = CreateEntityWithTransform();
        DataStore.i.sceneWorldObjects.AddExcludedOwner(scene.sceneData.sceneNumber, entity.entityId);

        DCLTexture texture = CreateTexture(texturePaths[0]);
        BasicMaterial material = CreateBasicMaterial(texture.id);
        PlaneShape planeShape = CreatePlane();

        yield return texture.routine;

        TestUtils.SharedComponentAttach(texture, entity);
        TestUtils.SharedComponentAttach(material, entity);
        TestUtils.SharedComponentAttach(planeShape, entity);

        var sceneMetrics = scene.metricsCounter.currentCount;

        Assert.That( sceneMetrics.materials, Is.EqualTo(0) );

        material.Dispose();
        texture.Dispose();
        planeShape.Dispose();
        DataStore.i.sceneWorldObjects.RemoveExcludedOwner(scene.sceneData.sceneNumber, entity.entityId);
    }


    [UnityTest]
    public IEnumerator CountWhenAdded()
    {
        IDCLEntity entity = CreateEntityWithTransform();
        BasicMaterial material = CreateBasicMaterial("");
        PlaneShape planeShape = CreatePlane();

        TestUtils.SharedComponentAttach(material, entity);
        TestUtils.SharedComponentAttach(planeShape, entity);

        yield return planeShape.routine;

        var sceneMetrics = scene.metricsCounter.currentCount;

        Assert.That( sceneMetrics.materials, Is.EqualTo(1) );

        material.Dispose();
        planeShape.Dispose();
    }

    [UnityTest]
    public IEnumerator CountWhenRemoved()
    {
        IDCLEntity entity = CreateEntityWithTransform();
        BasicMaterial material = CreateBasicMaterial("");
        PlaneShape planeShape = CreatePlane();

        TestUtils.SharedComponentAttach(material, entity);
        TestUtils.SharedComponentAttach(planeShape, entity);

        yield return planeShape.routine;

        material.Dispose();
        planeShape.Dispose();

        var sceneMetrics = scene.metricsCounter.currentCount;

        Assert.That( sceneMetrics.materials, Is.EqualTo(0) );
    }
}