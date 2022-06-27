using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = System.Diagnostics.Debug;
using Material = DCL.Helpers.Material;
using SkinnedMeshRenderer = UnityEngine.SkinnedMeshRenderer;

public class AvatarMeshCombinerCan
{
    private static string BASE_MALE_PATH = TestAssetsUtils.GetPath() + "/Avatar/Assets/BaseMale.glb";

    [UnityTest]
    public IEnumerator CombineSkinnedMeshes()
    {
        AssetPromiseKeeper_GLTF keeper = new AssetPromiseKeeper_GLTF();
        keeper.throttlingCounter.enabled = false;
        WebRequestController webRequestController = WebRequestController.Create();
        AssetPromise_GLTF promise = new AssetPromise_GLTF(BASE_MALE_PATH, webRequestController);

        keeper.Keep(promise);

        yield return promise;

        var renderersToCombine = promise.asset.container.GetComponentsInChildren<SkinnedMeshRenderer>();
        var firstRenderer = renderersToCombine[0];
        var materialAsset = Material.CreateOpaque();

        var output = AvatarMeshCombiner.CombineSkinnedMeshes(firstRenderer.sharedMesh.bindposes,
            firstRenderer.bones,
            renderersToCombine,
            materialAsset );

        Assert.That( output.isValid, Is.True );
        Assert.That( output.materials.Length, Is.EqualTo(2) );
        Assert.That( output.materials[0] != materialAsset, Is.True );
        Assert.That( output.mesh != null, Is.True );
        Assert.That( output.mesh.vertexCount, Is.EqualTo(1739) );

        keeper.Forget(promise);

        Object.Destroy(output.materials[0]);
        Object.Destroy(output.materials[1]);
        Object.Destroy(output.mesh);
        Object.Destroy(materialAsset);
    }

    [UnityTest]
    public IEnumerator CombineSkinnedMeshesKeepingPose()
    {
        AssetPromiseKeeper_GLTF keeper = new AssetPromiseKeeper_GLTF();
        keeper.throttlingCounter.enabled = false;
        WebRequestController webRequestController = WebRequestController.Create();
        AssetPromise_GLTF promise = new AssetPromise_GLTF(BASE_MALE_PATH, webRequestController);

        keeper.Keep(promise);

        yield return promise;

        var renderersToCombine = promise.asset.container.GetComponentsInChildren<SkinnedMeshRenderer>();
        var firstRenderer = renderersToCombine[0];
        var materialAsset = Material.CreateOpaque();
        (Vector3 pos, Quaternion rot, Vector3 scale)[] bonesTransforms = firstRenderer.bones.Select(x => (x.position, x.rotation, x.localScale)).ToArray();

        var output = AvatarMeshCombiner.CombineSkinnedMeshes(firstRenderer.sharedMesh.bindposes,
            firstRenderer.bones,
            renderersToCombine,
            materialAsset,
            true);

        for (int i = 0; i < bonesTransforms.Length; i++)
        {
            Assert.IsTrue(bonesTransforms[i].pos == firstRenderer.bones[i].transform.position);
            Assert.IsTrue(bonesTransforms[i].rot == firstRenderer.bones[i].transform.rotation);
            Assert.IsTrue(bonesTransforms[i].scale == firstRenderer.bones[i].transform.localScale);
        }

        keeper.Forget(promise);

        Object.Destroy(output.materials[0]);
        Object.Destroy(output.materials[1]);
        Object.Destroy(output.mesh);
        Object.Destroy(materialAsset);
    }

    [UnityTest]
    public IEnumerator CombineSkinnedMeshesNotKeepingPose()
    {
        AssetPromiseKeeper_GLTF keeper = new AssetPromiseKeeper_GLTF();
        keeper.throttlingCounter.enabled = false;
        WebRequestController webRequestController = WebRequestController.Create();
        AssetPromise_GLTF promise = new AssetPromise_GLTF(BASE_MALE_PATH, webRequestController);

        keeper.Keep(promise);

        yield return promise;

        var renderersToCombine = promise.asset.container.GetComponentsInChildren<SkinnedMeshRenderer>();
        var firstRenderer = renderersToCombine[0];
        var materialAsset = Material.CreateOpaque();
        (Vector3 pos, Quaternion rot, Vector3 scale)[] bonesTransforms = firstRenderer.bones.Select(x => (x.position, x.rotation, x.localScale)).ToArray();

        var output = AvatarMeshCombiner.CombineSkinnedMeshes(firstRenderer.sharedMesh.bindposes,
            firstRenderer.bones,
            renderersToCombine,
            materialAsset,
            false);

        for (int i = 0; i < bonesTransforms.Length; i++)
        {
            Assert.IsTrue(bonesTransforms[i].pos != firstRenderer.bones[i].transform.position);
            Assert.IsTrue(bonesTransforms[i].rot != firstRenderer.bones[i].transform.rotation);
        }

        keeper.Forget(promise);

        Object.Destroy(output.materials[0]);
        Object.Destroy(output.materials[1]);
        Object.Destroy(output.mesh);
        Object.Destroy(materialAsset);
    }
}