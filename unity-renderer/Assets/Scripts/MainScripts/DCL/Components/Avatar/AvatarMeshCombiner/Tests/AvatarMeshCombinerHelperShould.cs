using System.Collections;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Material = DCL.Helpers.Material;
using SkinnedMeshRenderer = UnityEngine.SkinnedMeshRenderer;

public class AvatarMeshCombinerHelperShould
{
    private static string BASE_MALE_PATH = TestAssetsUtils.GetPath() + "/Avatar/Assets/BaseMale.glb";

    private AssetPromiseKeeper_GLTF keeper;
    private WebRequestController webRequestController;
    private AssetPromise_GLTF promise;
    private SkinnedMeshRenderer bonesContainer;
    private SkinnedMeshRenderer[] renderersToCombine;
    private UnityEngine.Material materialAsset;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        keeper = new AssetPromiseKeeper_GLTF();
        webRequestController = WebRequestController.Create();
        promise = new AssetPromise_GLTF(BASE_MALE_PATH, webRequestController);

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
        yield break;
    }

    [Test]
    public void SetOriginalRenderersDisabledAfterCombining()
    {
        var helper = new AvatarMeshCombinerHelper();
        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        foreach ( var r in renderersToCombine )
        {
            Assert.That(r.enabled, Is.False);
        }

        helper.Dispose();
    }

    [UnityTest]
    public IEnumerator DisposeProperly()
    {
        var helper = new AvatarMeshCombinerHelper();

        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        var mesh = helper.renderer.sharedMesh;
        var mats = helper.renderer.sharedMaterials;

        Assert.That(helper.container != null, Is.True);
        Assert.That(helper.renderer.sharedMesh != null, Is.True);
        Assert.That(helper.renderer.sharedMaterials != null, Is.True);
        Assert.That(helper.renderer.sharedMaterials.Length, Is.GreaterThan(0));

        helper.Dispose();

        yield return null;

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
        var helper = new AvatarMeshCombinerHelper();
        renderersToCombine[0].sharedMesh = null;
        renderersToCombine[1].sharedMesh = null;
        renderersToCombine[2].sharedMesh = null;
        renderersToCombine[3].enabled = false;
        renderersToCombine[4].enabled = false;
        renderersToCombine[5] = null;
        renderersToCombine[6] = null;

        bool success = helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        Assert.That(success, Is.False);
        helper.Dispose();
    }

    [Test]
    public void SanitizeInvalidRenderers()
    {
        var helper = new AvatarMeshCombinerHelper();
        renderersToCombine[1].sharedMesh = null;
        renderersToCombine[2].enabled = false;
        renderersToCombine[3] = null;

        bool success = helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        Assert.That(success, Is.True);
        Assert.That(helper.renderer.sharedMesh != null, Is.True);
        Assert.That(helper.renderer.sharedMesh.vertexCount, Is.EqualTo(1542));

        helper.Dispose();
    }


    [Test]
    public void ConfigureFinalRendererSuccessfully()
    {
        var helper = new AvatarMeshCombinerHelper();
        bool success = helper.Combine(bonesContainer, renderersToCombine, materialAsset);

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
        var helper = new AvatarMeshCombinerHelper();
        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        for (int i = 0; i < renderersToCombine.Length; i++)
        {
            var renderer = renderersToCombine[i];
            renderer.enabled = true;
        }

        SkinnedMeshRenderer originalRenderer = helper.renderer;

        helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        Assert.That(originalRenderer == helper.renderer, Is.True, "Renderer should be reused!");

        helper.Dispose();
    }

    [UnityTest]
    public IEnumerator UnloadOldAssetsWhenCombineIsCalledMultipleTimes()
    {
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

        bool success = helper.Combine(bonesContainer, renderersToCombine, materialAsset);

        Assert.That(success, Is.True);

        yield return null;
        yield return null;
        yield return null;
        yield return null;

        Assert.That(oldMesh == null, Is.True);
        Assert.That(oldMaterial == null, Is.True);

        helper.Dispose();
    }
}