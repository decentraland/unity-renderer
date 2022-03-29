using System.Collections;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using UnityGLTF;

public class MemoryScoreFormulasShould
{
    [Test]
    public void OvershootTextureMemoryWhenScoreIsComputed()
    {
        var texture = new Texture2D(100, 100);
        texture.Apply(false, true);
        long score = MetricsScoreUtils.ComputeTextureScore(texture);
        // NOTE(Brian): We have to divide the expectancy by 2 because on editor/standalone runtime the
        //              reported memory is doubled when it shouldn't.
        Assert.That(score, Is.GreaterThan(Profiler.GetRuntimeMemorySizeLong(texture) / 2));
        Object.Destroy(texture);
    }

    [Test]
    public void OvershootAudioClipMemoryWhenScoreIsComputed()
    {
        var audioClip = AudioClip.Create("test", 10000, 2, 11000, false);
        long score = MetricsScoreUtils.ComputeAudioClipScore(audioClip);
        // NOTE(Brian): We have to divide the expectancy by 2 because on editor/standalone runtime the
        //              reported memory is doubled when it shouldn't.
        Assert.That(score, Is.GreaterThan(Profiler.GetRuntimeMemorySizeLong(audioClip) / 2));
        Object.Destroy(audioClip);
    }

    [UnityTest]
    public IEnumerator OvershootAnimationClipMemoryWhenScoreIsComputed()
    {
        AssetPromiseKeeper_GLTF keeper = new AssetPromiseKeeper_GLTF();
        keeper.throttlingCounter.enabled = false;
        IWebRequestController webRequestController = WebRequestController.Create();
        ContentProvider_Dummy provider = new ContentProvider_Dummy();

        string url = TestAssetsUtils.GetPath() + "/GLB/Trevor/Trevor.glb";

        AssetPromise_GLTF promise = new AssetPromise_GLTF(provider, url, webRequestController);
        promise.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITHOUT_TRANSITION;

        GameObject holder1 = new GameObject("Test1");
        promise.settings.parent = holder1.transform;
        keeper.Keep(promise);

        yield return promise;

        Debug.Log("Container: " + promise.asset.container.name);

        Animation animation = promise.asset.container.GetComponentInChildren<Animation>();
        AnimationClip clip = animation.clip;

        long animationClipEstimatedSize = promise.asset.animationClipSize;
        long animationClipRealSize = Profiler.GetRuntimeMemorySizeLong(clip);

        Debug.Log($"Animation clip real size: {animationClipRealSize} - Estimated: {animationClipEstimatedSize}");
        Assert.That(animationClipEstimatedSize, Is.GreaterThan(animationClipRealSize));

        keeper.Cleanup();
    }

    [UnityTest]
    public IEnumerator OvershootMeshMemoryWhenScoreIsComputed()
    {
        AssetPromiseKeeper_GLTF keeper = new AssetPromiseKeeper_GLTF();
        keeper.throttlingCounter.enabled = false;
        IWebRequestController webRequestController = WebRequestController.Create();
        ContentProvider_Dummy provider = new ContentProvider_Dummy();

        string url = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb";
        AssetPromise_GLTF promise = new AssetPromise_GLTF(provider, url, webRequestController);
        promise.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITHOUT_TRANSITION;

        GameObject holder1 = new GameObject("Test1");
        promise.settings.parent = holder1.transform;
        keeper.Keep(promise);

        yield return promise;

        long meshesEstimatedSize = promise.asset.meshDataSize;
        long meshesRealSize = 0;

        foreach (MeshFilter mf in holder1.GetComponentsInChildren<MeshFilter>())
        {
            meshesRealSize += Profiler.GetRuntimeMemorySizeLong(mf.sharedMesh);
        }

        Debug.Log($"Mesh real size: {meshesRealSize} - Estimated: {meshesEstimatedSize}");
        Assert.That(meshesEstimatedSize, Is.GreaterThan(meshesRealSize));

        keeper.Cleanup();
    }
}