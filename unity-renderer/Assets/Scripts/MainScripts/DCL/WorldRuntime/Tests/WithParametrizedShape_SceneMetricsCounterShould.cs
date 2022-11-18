using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WithParametrizedShape_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    public IEnumerator CountWhenChanged()
    {
        IDCLEntity entity = CreateEntityWithTransform();
        ConeShape coneShape = CreateCone();

        TestUtils.SharedComponentAttach(coneShape, entity);

        yield return coneShape.routine;

        AssertMetricsModel(scene,
            triangles: 101,
            materials: 0,
            entities: 1,
            meshes: 1,
            bodies: 1,
            textures: 0, "Failed before change");

        // TODO(Brian): right now, the segmentsRadial field is ignored.
        //              This test should be updated when parametrized shape vertex count
        //              changes with updates.
        TestUtils.SharedComponentUpdate(coneShape, new ConeShape.Model()
        {
            segmentsRadial = 18,
            radiusTop = 1,
            radiusBottom = 0
        });

        yield return coneShape.routine;
        yield return null;

        AssertMetricsModel(scene,
            triangles: 101,
            materials: 0,
            entities: 1,
            meshes: 1,
            bodies: 1,
            textures: 0, "Failed after change");
    }

    [UnityTest]
    public IEnumerator CountWhenRemoved()
    {
        ConeShape coneShape = CreateCone();
        PlaneShape planeShape = CreatePlane();
        DCLTexture dclTexture = CreateTexture(texturePaths[0]);
        BasicMaterial basicMaterial = CreateBasicMaterial(dclTexture.id);

        IDCLEntity entity = CreateEntityWithTransform();
        IDCLEntity entity2 = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(dclTexture, entity);
        TestUtils.SharedComponentAttach(basicMaterial, entity);
        TestUtils.SharedComponentAttach(basicMaterial, entity2);
        TestUtils.SharedComponentAttach(coneShape, entity);
        TestUtils.SharedComponentAttach(planeShape, entity2);

        yield return basicMaterial.routine;

        TestUtils.RemoveSceneEntity(scene, entity);
        TestUtils.RemoveSceneEntity(scene, entity2);
        dclTexture.Dispose();

        AssertMetricsModel(scene,
            triangles: 0,
            materials: 0,
            entities: 0,
            meshes: 0,
            bodies: 0,
            textures: 0);
    }

    [UnityTest]
    public IEnumerator CountWhenAdded()
    {
        ConeShape coneShape = CreateCone();
        PlaneShape planeShape = CreatePlane();
        DCLTexture dclTexture = CreateTexture(texturePaths[0]);
        BasicMaterial basicMaterial = CreateBasicMaterial(dclTexture.id);

        IDCLEntity entity = CreateEntityWithTransform();
        IDCLEntity entity2 = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(basicMaterial, entity);
        TestUtils.SharedComponentAttach(basicMaterial, entity2);
        TestUtils.SharedComponentAttach(coneShape, entity);
        TestUtils.SharedComponentAttach(planeShape, entity2);

        yield return basicMaterial.routine;

        AssertMetricsModel(scene,
            triangles: 105,
            materials: 1,
            entities: 2,
            meshes: 2,
            bodies: 2,
            textures: 1);
    }

    [UnityTest]
    public IEnumerator NotCountWhenAttachedToIgnoredEntities()
    {
        IDCLEntity entity = CreateEntityWithTransform();
        IDCLEntity entity2 = CreateEntityWithTransform();

        DataStore.i.sceneWorldObjects.AddExcludedOwner(scene.sceneData.sceneNumber, entity.entityId);

        ConeShape coneShape = CreateCone();
        PlaneShape planeShape = CreatePlane();

        TestUtils.SharedComponentAttach(coneShape, entity);
        TestUtils.SharedComponentAttach(planeShape, entity2);

        yield return planeShape.routine;

        AssertMetricsModel(scene,
            triangles: 4,
            materials: 0,
            entities: 1,
            meshes: 1,
            bodies: 1,
            textures: 0);


        DataStore.i.sceneWorldObjects.RemoveExcludedOwner(scene.sceneData.sceneNumber, entity.entityId);
    }
}