using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

public class BasicMaterialShould : TestsBase
{
    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        Environment.i.sceneBoundsChecker.Stop();
    }

    [UnityTest]
    public IEnumerator NotDestroySharedTextureWhenDisposed()
    {
        DCLTexture texture =
            TestHelpers.CreateDCLTexture(scene, Utils.GetTestsAssetsPath() + "/Images/atlas.png");

        yield return texture.routine;

        BasicMaterial mat = TestHelpers.CreateEntityWithBasicMaterial(scene,
            new BasicMaterial.Model
            {
                texture = texture.id,
                alphaTest = 1,
            },
            out DecentralandEntity entity1);

        yield return mat.routine;

        BasicMaterial mat2 = TestHelpers.CreateEntityWithBasicMaterial(scene,
            new BasicMaterial.Model
            {
                texture = texture.id,
                alphaTest = 1,
            },
            out DecentralandEntity entity2);

        yield return mat2.routine;

        TestHelpers.SharedComponentDispose(mat);
        Assert.IsTrue(texture.texture != null, "Texture should persist because is used by the other material!!");
    }

    [UnityTest]
    public IEnumerator WorkCorrectlyWhenAttachedBeforeShape()
    {
        DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

        DCLTexture dclTexture = TestHelpers.CreateDCLTexture(
            scene,
            Utils.GetTestsAssetsPath() + "/Images/atlas.png",
            DCLTexture.BabylonWrapMode.CLAMP,
            FilterMode.Bilinear);

        yield return dclTexture.routine;

        BasicMaterial mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>
        (scene, CLASS_ID.BASIC_MATERIAL,
            new BasicMaterial.Model
            {
                texture = dclTexture.id,
                alphaTest = 0.5f
            });

        yield return mat.routine;

        TestHelpers.SharedComponentAttach(mat, entity);

        SphereShape shape = TestHelpers.SharedComponentCreate<SphereShape, SphereShape.Model>(scene,
            CLASS_ID.SPHERE_SHAPE,
            new SphereShape.Model { });

        TestHelpers.SharedComponentAttach(shape, entity);

        Assert.IsTrue(entity.meshRootGameObject != null);
        Assert.IsTrue(entity.meshRootGameObject.GetComponent<MeshRenderer>() != null);
        Assert.AreEqual(entity.meshRootGameObject.GetComponent<MeshRenderer>().sharedMaterial, mat.material);
    }


    [UnityTest]
    public IEnumerator GetReplacedWhenAnotherMaterialIsAttached()
    {
        yield return
            TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<BasicMaterial.Model, BasicMaterial>(scene,
                CLASS_ID.BASIC_MATERIAL);
    }

    [UnityTest]
    public IEnumerator BeDetachedCorrectly()
    {
        string entityId = "1";
        string materialID = "a-material";

        TestHelpers.InstantiateEntityWithMaterial(scene, entityId, Vector3.zero,
            new BasicMaterial.Model(), materialID);

        Assert.IsTrue(scene.entities[entityId].meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        var meshRenderer = scene.entities[entityId].meshRootGameObject.GetComponent<MeshRenderer>();
        var materialComponent = scene.disposableComponents[materialID] as BasicMaterial;

        yield return materialComponent.routine;

        // Check if material initialized correctly
        {
            Assert.IsTrue(meshRenderer != null, "MeshRenderer must exist");

            Assert.AreEqual(meshRenderer.sharedMaterial, materialComponent.material, "Assigned material");
        }

        // Remove material
        materialComponent.DetachFrom(scene.entities[entityId]);

        // Check if material was removed correctly
        Assert.IsTrue(meshRenderer.sharedMaterial == null,
            "Assigned material should be null as it has been removed");
    }

    [UnityTest]
    public IEnumerator BeDetachedOnDispose()
    {
        string firstEntityId = "1";
        string secondEntityId = "2";
        string materialID = "a-material";

        // Instantiate entity with material
        TestHelpers.InstantiateEntityWithMaterial(scene, firstEntityId, Vector3.zero,
            new BasicMaterial.Model(), materialID);

        Assert.IsTrue(scene.entities[firstEntityId].meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        // Create 2nd entity and attach same material to it
        TestHelpers.InstantiateEntityWithShape(scene, secondEntityId, CLASS_ID.BOX_SHAPE, Vector3.zero);
        scene.SharedComponentAttach(
            secondEntityId,
            materialID
        );

        Assert.IsTrue(scene.entities[secondEntityId].meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        var firstMeshRenderer = scene.entities[firstEntityId].meshRootGameObject.GetComponent<MeshRenderer>();
        var secondMeshRenderer = scene.entities[secondEntityId].meshRootGameObject.GetComponent<MeshRenderer>();
        var materialComponent = scene.disposableComponents[materialID] as DCL.Components.BasicMaterial;

        yield return materialComponent.routine;

        // Check if material attached correctly
        {
            Assert.IsTrue(firstMeshRenderer != null, "MeshRenderer must exist");
            Assert.AreEqual(firstMeshRenderer.sharedMaterial, materialComponent.material, "Assigned material");

            Assert.IsTrue(secondMeshRenderer != null, "MeshRenderer must exist");
            Assert.AreEqual(secondMeshRenderer.sharedMaterial, materialComponent.material, "Assigned material");
        }

        // Dispose material
        scene.SharedComponentDispose(materialID);

        // Check if material detached correctly
        Assert.IsTrue(firstMeshRenderer.sharedMaterial == null, "MeshRenderer must exist");
        Assert.IsTrue(secondMeshRenderer.sharedMaterial == null, "MeshRenderer must exist");
    }

    [UnityTest]
    public IEnumerator EntityBasicMaterialUpdate()
    {
        string entityId = "1";
        string materialID = "a-material";

        Assert.IsFalse(scene.disposableComponents.ContainsKey(materialID));

        // Instantiate entity with default material
        TestHelpers.InstantiateEntityWithMaterial(scene, entityId, new Vector3(8, 1, 8),
            new BasicMaterial.Model(), materialID);

        var meshObject = scene.entities[entityId].meshRootGameObject;
        Assert.IsTrue(meshObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        var meshRenderer = meshObject.GetComponent<MeshRenderer>();
        var materialComponent = scene.disposableComponents[materialID] as BasicMaterial;

        yield return materialComponent.routine;

        // Check if material initialized correctly
        {
            Assert.IsTrue(meshRenderer != null, "MeshRenderer must exist");

            var assignedMaterial = meshRenderer.sharedMaterial;
            Assert.IsTrue(meshRenderer != null, "MeshRenderer.sharedMaterial must be the same as assignedMaterial");

            Assert.AreEqual(assignedMaterial, materialComponent.material, "Assigned material");
        }

        // Check default properties
        {
            Assert.IsTrue(materialComponent.material.GetTexture("_BaseMap") == null);
            Assert.AreApproximatelyEqual(1.0f, materialComponent.material.GetFloat("_AlphaClip"));
        }

        DCLTexture dclTexture = TestHelpers.CreateDCLTexture(
            scene,
            Utils.GetTestsAssetsPath() + "/Images/atlas.png",
            DCLTexture.BabylonWrapMode.MIRROR,
            FilterMode.Bilinear);

        // Update material
        scene.SharedComponentUpdate(materialID, JsonUtility.ToJson(new BasicMaterial.Model
        {
            texture = dclTexture.id,
            alphaTest = 0.5f,
        }));

        yield return materialComponent.routine;

        // Check updated properties
        {
            Texture mainTex = materialComponent.material.GetTexture("_BaseMap");
            Assert.IsTrue(mainTex != null);
            Assert.AreApproximatelyEqual(0.5f, materialComponent.material.GetFloat("_Cutoff"));
            Assert.AreApproximatelyEqual(1.0f, materialComponent.material.GetFloat("_AlphaClip"));
            Assert.AreEqual(TextureWrapMode.Mirror, mainTex.wrapMode);
            Assert.AreEqual(FilterMode.Bilinear, mainTex.filterMode);
        }
    }

    [UnityTest]
    public IEnumerator DefaultMissingValuesPropertyOnUpdate()
    {
        // 1. Create component with non-default configs
        BasicMaterial basicMaterialComponent =
            TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model
                {
                    alphaTest = 1f
                });

        yield return basicMaterialComponent.routine;

        // 2. Check configured values
        Assert.AreEqual(1f, basicMaterialComponent.model.alphaTest);

        // 3. Update component with missing values

        scene.SharedComponentUpdate(basicMaterialComponent.id, JsonUtility.ToJson(new BasicMaterial.Model { }));

        yield return basicMaterialComponent.routine;

        // 4. Check defaulted values
        Assert.AreEqual(0.5f, basicMaterialComponent.model.alphaTest);
    }

    [UnityTest]
    public IEnumerator ProcessCastShadowProperty_True()
    {
        BasicMaterial basicMaterialComponent = TestHelpers.CreateEntityWithBasicMaterial(scene, new BasicMaterial.Model
        {
            alphaTest = 1f,
            castShadows = true
        }, out DecentralandEntity entity);
        yield return basicMaterialComponent.routine;

        Assert.AreEqual(true, basicMaterialComponent.model.castShadows);
        Assert.AreEqual(ShadowCastingMode.On, entity.meshRootGameObject.GetComponent<MeshRenderer>().shadowCastingMode);
    }

    [UnityTest]
    public IEnumerator ProcessCastShadowProperty_False()
    {
        BasicMaterial basicMaterialComponent = TestHelpers.CreateEntityWithBasicMaterial(scene, new BasicMaterial.Model
        {
            alphaTest = 1f,
            castShadows = false
        }, out DecentralandEntity entity);
        yield return basicMaterialComponent.routine;

        Assert.AreEqual(false, basicMaterialComponent.model.castShadows);
        Assert.AreEqual(ShadowCastingMode.Off, entity.meshRootGameObject.GetComponent<MeshRenderer>().shadowCastingMode);
    }
}