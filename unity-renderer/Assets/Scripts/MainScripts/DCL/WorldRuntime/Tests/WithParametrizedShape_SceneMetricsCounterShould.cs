using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithParametrizedShape_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    public IEnumerator CountWhenChanged()
    {
        Assert.Fail();
        yield return null;
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
        Assert.Fail();
        yield return null;
    }
}