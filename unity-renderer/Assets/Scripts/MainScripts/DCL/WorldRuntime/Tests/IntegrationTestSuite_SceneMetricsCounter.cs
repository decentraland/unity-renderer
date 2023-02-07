using System.Collections;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class IntegrationTestSuite_SceneMetricsCounter : IntegrationTestSuite
{
    protected ParcelScene scene;
    private CoreComponentsPlugin coreComponentsPlugin;

    protected readonly string[] texturePaths =
    {
        "/Images/alphaTexture.png",
        "/Images/atlas.png",
        "/Images/avatar.png",
        "/Images/avatar2.png"
    };

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
        serviceLocator.Register<IParcelScenesCleaner>(() => new ParcelScenesCleaner());
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        coreComponentsPlugin = new CoreComponentsPlugin();
        scene = TestUtils.CreateTestScene();
        scene.contentProvider = new ContentProvider_Dummy();

        // TODO(Brian): Move these variants to a DataStore object to avoid having to reset them
        //              like this.
        CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
        CommonScriptableObjects.rendererState.Set(true);
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        scene.Cleanup(true);
        yield return base.TearDown();
    }

    protected PlaneShape CreatePlane()
    {
        PlaneShape planeShape = TestUtils.SharedComponentCreate<PlaneShape, PlaneShape.Model>(
            scene,
            DCL.Models.CLASS_ID.PLANE_SHAPE,
            new PlaneShape.Model()
            {
                height = 1.5f,
                width = 1
            }
        );
        return planeShape;
    }

    protected ConeShape CreateCone()
    {
        ConeShape coneShape = TestUtils.SharedComponentCreate<ConeShape, ConeShape.Model>(
            scene,
            DCL.Models.CLASS_ID.CONE_SHAPE,
            new ConeShape.Model()
            {
                radiusTop = 1,
                radiusBottom = 0
            }
        );
        return coneShape;
    }

    protected PBRMaterial CreatePBRMaterial( string albedoTexId, string alphaTexId, string bumpTexId, string emissiveTexId )
    {
        PBRMaterial basicMaterial = TestUtils.SharedComponentCreate<PBRMaterial, PBRMaterial.Model>(
            scene,
            DCL.Models.CLASS_ID.PBR_MATERIAL,
            new PBRMaterial.Model()
            {
                albedoTexture = albedoTexId,
                alphaTexture = alphaTexId,
                bumpTexture = bumpTexId,
                emissiveTexture = emissiveTexId
            });
        return basicMaterial;
    }

    protected BasicMaterial CreateBasicMaterial( string textureId )
    {
        BasicMaterial basicMaterial = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(
            scene,
            DCL.Models.CLASS_ID.BASIC_MATERIAL,
            new BasicMaterial.Model()
            {
                texture = textureId
            });
        return basicMaterial;
    }

    protected DCLTexture CreateTexture( string path )
    {
        return TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + path);
    }

    protected IDCLEntity CreateEntityWithTransform()
    {
        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
        TestUtils.SetEntityTransform(scene, entity, Vector3.one, Quaternion.identity, Vector3.one);
        return entity;
    }

    protected void AssertMetricsModel(ParcelScene scene, int triangles, int materials, int entities, int meshes, int bodies,
        int textures, string failMessage = "")
    {
        SceneMetricsModel inputModel = scene.metricsCounter.currentCount;

        Assert.AreEqual(triangles, inputModel.triangles, "Incorrect triangle count ... " + failMessage);
        Assert.AreEqual(materials, inputModel.materials, "Incorrect materials count ... " + failMessage);
        Assert.AreEqual(entities, inputModel.entities, "Incorrect entities count ... " + failMessage);
        Assert.AreEqual(meshes, inputModel.meshes, "Incorrect geometries/meshes count ... " + failMessage);
        Assert.AreEqual(bodies, inputModel.bodies, "Incorrect bodies count ... " + failMessage);
        Assert.AreEqual(textures, inputModel.textures, "Incorrect textures count ... " + failMessage);
    }
}
