using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Material = DCL.Helpers.Material;
using SkinnedMeshRenderer = UnityEngine.SkinnedMeshRenderer;

public class AvatarMeshCombinerHelperShould
{
    private static string BASE_MALE_PATH = TestAssetsUtils.GetPath() + "/Avatar/Assets/BaseMale.glb";

    private AssetPromiseKeeper_GLTFast_Instance keeper;
    private WebRequestController webRequestController;
    private AssetPromise_GLTFast_Instance promise;
    private SkinnedMeshRenderer bonesContainer;
    private SkinnedMeshRenderer[] renderersToCombine;
    private UnityEngine.Material materialAsset;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var serviceLocator = ServiceLocatorFactory.CreateDefault();
        Environment.Setup(serviceLocator);

        keeper = new AssetPromiseKeeper_GLTFast_Instance();
        webRequestController = WebRequestController.Create();
        promise = new AssetPromise_GLTFast_Instance("", BASE_MALE_PATH, webRequestController);

        yield return keeper.Keep(promise);

        renderersToCombine = promise.asset.container.GetComponentsInChildren<SkinnedMeshRenderer>();
        bonesContainer = renderersToCombine[0];
        materialAsset = Material.CreateOpaque();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        keeper.Forget(promise);
        webRequestController.Dispose();
        Object.Destroy(materialAsset);
        PoolManager.i.Dispose();
        Environment.Dispose();
        yield break;
    }

    [Test]
    public void SetOriginalRenderersDisabledAfterCombining()
    {
        // Arrange
        var helper = new AvatarMeshCombinerHelper();

        // Act
        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        // Assert
        foreach ( var r in renderersToCombine )
        {
            Assert.That(r.enabled, Is.False);
        }

        helper.Dispose();
    }

    [UnityTest]
    public IEnumerator DisposeProperly()
    {
        // Arrange
        var helper = new AvatarMeshCombinerHelper();

        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        var mesh = helper.renderer.sharedMesh;
        var mats = helper.renderer.sharedMaterials;

        Assert.That(helper.container != null, Is.True);
        Assert.That(helper.renderer.sharedMesh != null, Is.True);
        Assert.That(helper.renderer.sharedMaterials != null, Is.True);
        Assert.That(helper.renderer.sharedMaterials.Length, Is.GreaterThan(0));

        // Act
        helper.Dispose();
        yield return null;

        // Assert
        Assert.That(helper.container == null, Is.True);
        Assert.That(mesh == null, Is.True);

        foreach ( var mat in mats )
        {
            Assert.That(mat == null, Is.True);
        }
    }

    [Test]
    public void FailWithNoValidRenderers()
    {
        // Arrange
        var helper = new AvatarMeshCombinerHelper();

        Mesh origMesh1 = renderersToCombine[0].sharedMesh;
        Mesh origMesh2 = renderersToCombine[1].sharedMesh;
        Mesh origMesh3 = renderersToCombine[2].sharedMesh;

        SkinnedMeshRenderer origSkr1 = renderersToCombine[5];
        SkinnedMeshRenderer origSkr2 = renderersToCombine[6];

        renderersToCombine[0].sharedMesh = null;
        renderersToCombine[1].sharedMesh = null;
        renderersToCombine[2].sharedMesh = null;
        renderersToCombine[3].sharedMesh = null;
        renderersToCombine[4] = null;
        renderersToCombine[5] = null;
        renderersToCombine[6] = null;

        // Act
        bool success = helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        // Assert
        Assert.That(success, Is.False);

        helper.Dispose();
        renderersToCombine[5] = origSkr1;
        renderersToCombine[6] = origSkr2;
        renderersToCombine[0].sharedMesh = origMesh1;
        renderersToCombine[1].sharedMesh = origMesh2;
        renderersToCombine[2].sharedMesh = origMesh3;
    }

    [Test]
    public void SanitizeInvalidRenderers()
    {
        // Arrange
        var helper = new AvatarMeshCombinerHelper();

        Mesh origMesh1 = renderersToCombine[1].sharedMesh;
        SkinnedMeshRenderer origSkr1 = renderersToCombine[3];

        renderersToCombine[1].sharedMesh = null;
        renderersToCombine[2].sharedMesh = null;
        renderersToCombine[3] = null;

        // Act
        bool success = helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(helper.renderer.sharedMesh != null, Is.True);
        Assert.That(helper.renderer.sharedMesh.vertexCount, Is.EqualTo(1220));

        helper.Dispose();

        renderersToCombine[1].sharedMesh = origMesh1;
        renderersToCombine[3] = origSkr1;
    }

    [Test]
    public void ConfigureFinalRendererSuccessfully()
    {
        // Arrange
        var helper = new AvatarMeshCombinerHelper();

        // Act
        bool success = helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(helper.renderer.sharedMesh != null, Is.True);
        Assert.That(helper.renderer.sharedMesh.vertexCount, Is.EqualTo(1739));
        Assert.That(helper.renderer.sharedMaterials.Length, Is.EqualTo(2));
        Assert.That(helper.renderer.rootBone != null, Is.True);
        Assert.That(helper.renderer.enabled, Is.True);
        Assert.That(helper.renderer.gameObject.layer, Is.EqualTo(bonesContainer.gameObject.layer));

        helper.Dispose();
    }

    [Test]
    public void ReuseSameRendererWhenCombineIsCalledMultipleTimes()
    {
        // Arrange
        var helper = new AvatarMeshCombinerHelper();
        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        for (int i = 0; i < renderersToCombine.Length; i++)
        {
            var renderer = renderersToCombine[i];
            renderer.enabled = true;
        }

        SkinnedMeshRenderer originalRenderer = helper.renderer;

        // Act
        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        // Assert
        Assert.That(originalRenderer == helper.renderer, Is.True, "Renderer should be reused!");

        helper.Dispose();
    }

    [UnityTest]
    public IEnumerator UnloadOldAssetsWhenCombineIsCalledMultipleTimes()
    {
        // Arrange
        var helper = new AvatarMeshCombinerHelper();
        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        Mesh oldMesh = helper.renderer.sharedMesh;
        UnityEngine.Material oldMaterial = helper.renderer.sharedMaterials[0];

        Assert.That(oldMesh != null, Is.True);
        Assert.That(oldMaterial != null, Is.True);

        for (int i = 0; i < renderersToCombine.Length; i++)
        {
            var renderer = renderersToCombine[i];
            renderer.enabled = true;
        }

        // Act
        bool success = helper.Combine(bonesContainer, renderersToCombine, materialAsset);
        yield return null;

        // Assert
        Assert.That(success, Is.True);
        Assert.That(oldMesh == null, Is.True);
        Assert.That(oldMaterial == null, Is.True);

        helper.Dispose();
    }
}
