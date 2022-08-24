using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Linq;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class PBRMaterialShould : IntegrationTestSuite_Legacy
{
    private ParcelScene scene;
    private CoreComponentsPlugin coreComponentsPlugin;
    
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();
        coreComponentsPlugin = new CoreComponentsPlugin();
        Environment.i.world.sceneBoundsChecker.Stop();
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator NotDestroySharedTextureWhenDisposed()
    {
        DCLTexture texture =
            TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/atlas.png");

        yield return texture.routine;

        PBRMaterial mat = TestUtils.CreateEntityWithPBRMaterial(scene,
            new PBRMaterial.Model
            {
                albedoTexture = texture.id,
                metallic = 0,
                roughness = 1,
            },
            out IDCLEntity entity1);

        yield return mat.routine;

        PBRMaterial mat2 = TestUtils.CreateEntityWithPBRMaterial(scene,
            new PBRMaterial.Model
            {
                albedoTexture = texture.id,
                metallic = 0,
                roughness = 1,
            },
            out IDCLEntity entity2);

        yield return mat2.routine;

        TestUtils.SharedComponentDispose(mat);
        Assert.IsTrue(texture.texture != null, "Texture should persist because is used by the other material!!");
    }

    [UnityTest]
    public IEnumerator BeCreatedProperly()
    {
        DCLTexture texture =
            TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/atlas.png");

        yield return texture.routine;

        PBRMaterial matPBR = TestUtils.CreateEntityWithPBRMaterial(scene,
            new PBRMaterial.Model
            {
                albedoTexture = texture.id,
                metallic = 0,
                roughness = 1,
            },
            out IDCLEntity entity);

        yield return matPBR.routine;

        Assert.IsTrue(entity.meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        var meshRenderer = entity.meshRootGameObject.GetComponent<MeshRenderer>();
        Assert.IsTrue(meshRenderer != null, "MeshRenderer must exist");

        var assignedMaterial = meshRenderer.sharedMaterial;
        Assert.IsTrue(meshRenderer != null, "MeshRenderer.sharedMaterial must be the same as assignedMaterial");
        Assert.AreEqual(assignedMaterial, matPBR.material, "Assigned material");

        var loadedTexture = meshRenderer.sharedMaterial.GetTexture("_BaseMap");
        Assert.IsTrue(loadedTexture != null, "Texture must be loaded");
        Assert.AreEqual(texture.texture, loadedTexture, "Texture data must be correct");
    }

    [UnityTest]
    public IEnumerator BeUpdatedProperly()
    {
        long entityId = 1;
        string materialID = "a-material";

        // Instantiate entity with default PBR Material
        TestUtils.InstantiateEntityWithMaterial(scene, entityId, Vector3.zero,
            new PBRMaterial.Model(), materialID);

        var materialComponent = scene.componentsManagerLegacy.GetSceneSharedComponent(materialID) as DCL.Components.PBRMaterial;

        yield return materialComponent.routine;

        Assert.IsTrue(materialComponent is DCL.Components.PBRMaterial, "material is PBRMaterial");

        // Check if material initialized correctly
        {
            Assert.IsTrue(scene.entities[entityId].meshRootGameObject != null,
                "Every entity with a shape should have the mandatory 'Mesh' object as a child");

            var meshRenderer = scene.entities[entityId].meshRootGameObject.GetComponent<MeshRenderer>();

            Assert.IsTrue(meshRenderer != null, "MeshRenderer must exist");

            var assignedMaterial = meshRenderer.sharedMaterial;
            Assert.IsTrue(meshRenderer != null, "MeshRenderer.sharedMaterial must be the same as assignedMaterial");

            Assert.AreEqual(assignedMaterial, materialComponent.material, "Assigned material");
        }

        // Check default properties
        {
            // Texture
            Assert.IsTrue(materialComponent.material.GetTexture("_BaseMap") == null);

            // Colors
            Assert.AreEqual("FFFFFF", ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_BaseColor")));
            Assert.AreEqual("000000",
                ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_EmissionColor")));
            Assert.AreEqual("FFFFFF",
                ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_SpecColor")));

            // Other properties
            Assert.AreApproximatelyEqual(0.5f, materialComponent.material.GetFloat("_Metallic"));
            Assert.AreApproximatelyEqual(0.5f, materialComponent.material.GetFloat("_Smoothness"));
            Assert.AreApproximatelyEqual(1.0f, materialComponent.material.GetFloat("_EnvironmentReflections"));
            Assert.AreApproximatelyEqual(1.0f, materialComponent.material.GetFloat("_SpecularHighlights"));
            Assert.AreApproximatelyEqual(.0f, materialComponent.material.GetFloat("_AlphaClip"));
        }

        // Update material
        DCLTexture texture =
            TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/atlas.png");

        yield return texture.routine;

        Color color1, color2, color3;

        ColorUtility.TryParseHtmlString("#99deff", out color1);
        ColorUtility.TryParseHtmlString("#42f4aa", out color2);
        ColorUtility.TryParseHtmlString("#601121", out color3);

        scene.componentsManagerLegacy.SceneSharedComponentUpdate(materialID, JsonUtility.ToJson(new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = texture.id,
            albedoColor = color1,
            emissiveColor = color2,
            emissiveIntensity = 1,
            reflectivityColor = color3,
            metallic = 0.37f,
            roughness = 0.9f,
            microSurface = 0.4f,
            specularIntensity = 2f,
            transparencyMode = 2,
        }));

        yield return materialComponent.routine;

        // Check updated properties
        {
            // Texture
            Assert.IsTrue(materialComponent.material.GetTexture("_BaseMap") != null, "texture is null!");

            // Colors
            Assert.AreEqual("99DEFF", ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_BaseColor")));
            Assert.AreEqual("42F4AA",
                ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_EmissionColor")));
            Assert.AreEqual("601121",
                ColorUtility.ToHtmlStringRGB(materialComponent.material.GetColor("_SpecColor")));

            // Other properties
            Assert.AreApproximatelyEqual(0.37f, materialComponent.material.GetFloat("_Metallic"));
            Assert.AreApproximatelyEqual(0.1f, materialComponent.material.GetFloat("_Smoothness"));
            Assert.AreApproximatelyEqual(0.4f, materialComponent.material.GetFloat("_EnvironmentReflections"));
            Assert.AreApproximatelyEqual(2.0f, materialComponent.material.GetFloat("_SpecularHighlights"));
            Assert.AreApproximatelyEqual(.0f, materialComponent.material.GetFloat("_AlphaClip"));
            Assert.AreEqual((int) UnityEngine.Rendering.BlendMode.SrcAlpha,
                materialComponent.material.GetInt("_SrcBlend"));
            Assert.AreEqual((int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,
                materialComponent.material.GetInt("_DstBlend"));
            Assert.AreEqual(0, materialComponent.material.GetInt("_ZWrite"));
        }
    }

    [UnityTest]
    public IEnumerator BeSharedProperly()
    {
        // Create first entity with material
        long firstEntityID = 1;
        string firstMaterialID = "a-material";

        TestUtils.InstantiateEntityWithMaterial(scene, firstEntityID, Vector3.zero,
            new DCL.Components.PBRMaterial.Model
            {
                metallic = 0.3f,
            }, firstMaterialID);

        Assert.IsTrue(scene.entities[firstEntityID].meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        // Create second entity with material
        long secondEntityID = 2;
        string secondMaterialID = "b-material";

        TestUtils.InstantiateEntityWithMaterial(scene, secondEntityID, Vector3.zero,
            new DCL.Components.PBRMaterial.Model
            {
                metallic = 0.66f,
            }, secondMaterialID);

        Assert.IsTrue(scene.entities[secondEntityID].meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        // Create third entity and assign 1st material
        long thirdEntityID = 3;

        TestUtils.InstantiateEntityWithShape(scene, thirdEntityID, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);
        scene.componentsManagerLegacy.SceneSharedComponentAttach(
            thirdEntityID,
            firstMaterialID
        );

        Assert.IsTrue(scene.entities[thirdEntityID].meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        // Check renderers material references
        var firstRenderer = scene.entities[firstEntityID].meshRootGameObject.GetComponent<MeshRenderer>();
        var secondRenderer = scene.entities[secondEntityID].meshRootGameObject.GetComponent<MeshRenderer>();
        var thirdRenderer = scene.entities[thirdEntityID].meshRootGameObject.GetComponent<MeshRenderer>();
        Assert.IsTrue(firstRenderer.sharedMaterial != secondRenderer.sharedMaterial,
            "1st and 2nd entities should have different materials");
        Assert.IsTrue(firstRenderer.sharedMaterial == thirdRenderer.sharedMaterial,
            "1st and 3rd entities should have the same material");

        yield break;
    }

    [UnityTest]
    public IEnumerator AffectDifferentEntitiesCorrectly()
    {
        // Create first entity with material
        PBRMaterial material1 = TestUtils.CreateEntityWithPBRMaterial(scene,
            new PBRMaterial.Model
            {
                metallic = 0.3f,
            }, out IDCLEntity entity1);

        Assert.IsTrue(entity1.meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        // Create second entity with material
        PBRMaterial material2 = TestUtils.CreateEntityWithPBRMaterial(scene,
            new PBRMaterial.Model
            {
                metallic = 0.66f,
            }, out IDCLEntity entity2);

        Assert.IsTrue(entity2.meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        // Create third entity and assign 1st material
        var boxShape = TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero);
        var entity3 = boxShape.attachedEntities.First();

        scene.componentsManagerLegacy.SceneSharedComponentAttach(
            entity3.entityId,
            material1.id
        );

        Assert.IsTrue(entity3.meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        // Check renderers material references
        var firstRenderer = entity1.meshRootGameObject.GetComponent<MeshRenderer>();
        var secondRenderer = entity2.meshRootGameObject.GetComponent<MeshRenderer>();
        var thirdRenderer = entity3.meshRootGameObject.GetComponent<MeshRenderer>();

        Assert.IsTrue(firstRenderer.sharedMaterial != secondRenderer.sharedMaterial,
            "1st and 2nd entities should have different materials");
        Assert.IsTrue(firstRenderer.sharedMaterial == thirdRenderer.sharedMaterial,
            "1st and 3rd entities should have the same material");

        // Check material properties before updating them
        Assert.AreApproximatelyEqual(0.3f, firstRenderer.sharedMaterial.GetFloat("_Metallic"));
        Assert.AreApproximatelyEqual(0.66f, secondRenderer.sharedMaterial.GetFloat("_Metallic"));

        // Update material properties
        scene.componentsManagerLegacy.SceneSharedComponentUpdate(material1.id, JsonUtility.ToJson(new PBRMaterial.Model
        {
            metallic = 0.95f
        }));

        Assert.IsTrue(firstRenderer.sharedMaterial != null);

        yield return material1.routine;

        Assert.IsTrue(firstRenderer.sharedMaterial != null);
        // Check material properties after updating them
        Assert.AreApproximatelyEqual(0.95f, firstRenderer.sharedMaterial.GetFloat("_Metallic"));
        Assert.AreApproximatelyEqual(0.66f, secondRenderer.sharedMaterial.GetFloat("_Metallic"));
    }

    [UnityTest]
    public IEnumerator WorkCorrectlyWhenAttachedBeforeShape()
    {
        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);

        DCLTexture dclTexture = TestUtils.CreateDCLTexture(
            scene,
            TestAssetsUtils.GetPath() + "/Images/atlas.png",
            DCLTexture.BabylonWrapMode.CLAMP,
            FilterMode.Bilinear);

        yield return dclTexture.routine;

        PBRMaterial mat = TestUtils.SharedComponentCreate<PBRMaterial, PBRMaterial.Model>(scene,
            CLASS_ID.PBR_MATERIAL,
            new PBRMaterial.Model
            {
                albedoTexture = dclTexture.id,
                metallic = 0,
                roughness = 1,
            }
        );

        yield return mat.routine;

        TestUtils.SharedComponentAttach(mat, entity);

        SphereShape shape = TestUtils.SharedComponentCreate<SphereShape, SphereShape.Model>(scene,
            CLASS_ID.SPHERE_SHAPE,
            new SphereShape.Model { });

        yield return shape.routine;

        TestUtils.SharedComponentAttach(shape, entity);
        
        yield return null;

        Assert.IsTrue(entity.meshRootGameObject != null);
        Assert.IsTrue(entity.meshRootGameObject.GetComponent<MeshRenderer>() != null);
        Assert.AreEqual(entity.meshRootGameObject.GetComponent<MeshRenderer>().sharedMaterial, mat.material);
    }

    [UnityTest]
    public IEnumerator DefaultMissingValuesPropertyOnUpdate()
    {
        Color color1;
        ColorUtility.TryParseHtmlString("#808080", out color1);

        // 1. Create component with non-default configs
        PBRMaterial PBRMaterialComponent = TestUtils.SharedComponentCreate<PBRMaterial, PBRMaterial.Model>(scene,
            CLASS_ID.PBR_MATERIAL,
            new PBRMaterial.Model
            {
                albedoColor = color1,
                metallic = 0.3f,
                directIntensity = 0.1f,
                specularIntensity = 3f
            });

        yield return PBRMaterialComponent.routine;

        // 2. Check configured values
        Assert.AreEqual(color1, PBRMaterialComponent.GetModel().albedoColor);
        Assert.AreEqual(0.3f, PBRMaterialComponent.GetModel().metallic);
        Assert.AreEqual(0.1f, PBRMaterialComponent.GetModel().directIntensity);
        Assert.AreEqual(3f, PBRMaterialComponent.GetModel().specularIntensity);

        // 3. Update component with missing values
        scene.componentsManagerLegacy.SceneSharedComponentUpdate(PBRMaterialComponent.id, JsonUtility.ToJson(new PBRMaterial.Model { }));

        yield return PBRMaterialComponent.routine;

        // 4. Check defaulted values
        Assert.AreEqual(Color.white, PBRMaterialComponent.GetModel().albedoColor);
        Assert.AreEqual(0.5f, PBRMaterialComponent.GetModel().metallic);
        Assert.AreEqual(1, PBRMaterialComponent.GetModel().directIntensity);
        Assert.AreEqual(1f, PBRMaterialComponent.GetModel().specularIntensity);
    }

    [UnityTest]
    public IEnumerator GetReplacedWhenAnotherMaterialIsAttached()
    {
        yield return TestUtils.TestAttachedSharedComponentOfSameTypeIsReplaced<PBRMaterial.Model, PBRMaterial>(
            scene, CLASS_ID.PBR_MATERIAL);
    }
}