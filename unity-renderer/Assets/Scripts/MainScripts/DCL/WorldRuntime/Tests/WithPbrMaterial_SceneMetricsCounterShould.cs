using System.Collections;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithPbrMaterial_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    public IEnumerator NotCountWhenAttachedToIgnoredEntities()
    {
        var material = CreatePBRMaterial("", "", "", "");
        var shape = CreatePlane();
        var entity = CreateEntityWithTransform();

        DataStore.i.sceneWorldObjects.AddExcludedOwner(scene.sceneData.sceneNumber, entity.entityId);

        TestUtils.SharedComponentAttach(shape, entity);
        TestUtils.SharedComponentAttach(material, entity);

        yield return material.routine;

        Assert.That( scene.metricsCounter.currentCount.entities, Is.EqualTo(0) );
        Assert.That( scene.metricsCounter.currentCount.materials, Is.EqualTo(0) );

        DataStore.i.sceneWorldObjects.RemoveExcludedOwner(scene.sceneData.sceneNumber, entity.entityId);
    }


    [UnityTest]
    public IEnumerator CountWhenAdded()
    {
        var material = CreatePBRMaterial("", "", "", "");
        var shape = CreatePlane();
        var entity = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(shape, entity);
        TestUtils.SharedComponentAttach(material, entity);

        yield return material.routine;

        Assert.That( scene.metricsCounter.currentCount.materials, Is.EqualTo(1) );
        Assert.That( scene.metricsCounter.currentCount.bodies, Is.EqualTo(1) );
        Assert.That( scene.metricsCounter.currentCount.entities, Is.EqualTo(1) );
    }

    [UnityTest]
    public IEnumerator CountWhenRemoved()
    {
        var material = CreatePBRMaterial("", "", "", "");
        var shape = CreatePlane();
        var entity = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(shape, entity);
        TestUtils.SharedComponentAttach(material, entity);

        yield return material.routine;

        material.Dispose();
        shape.Dispose();

        Assert.That( scene.metricsCounter.currentCount.materials, Is.EqualTo(0) );
        Assert.That( scene.metricsCounter.currentCount.bodies, Is.EqualTo(0) );
        Assert.That( scene.metricsCounter.currentCount.entities, Is.EqualTo(0) );
    }

    [UnityTest]
    public IEnumerator NotCountWhenNoShapeIsPresent()
    {
        var material1 = CreatePBRMaterial("", "", "", "");

        yield return material1.routine;

        Assert.That( scene.metricsCounter.currentCount.materials, Is.EqualTo(0) );
    }
}