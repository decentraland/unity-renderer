using DCL;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class purpose is to test GLTF importer stability, performance, memory consumption and leaks
/// </summary>
public class APK_GLTF_InteractiveTest : MonoBehaviour
{
    private AssetPromiseKeeper_GLTF apkGltf;
    private AssetPromiseKeeper_GLTFast_Instance apkGltFast;
    private WebRequestController webRequestController;
    private readonly List<AssetPromise_GLTF> gltfPromises = new ();
    private readonly List<AssetPromise_GLTFast_Instance> gltFastPromises = new ();

    private int counter = 0;
    private bool automatedMode = false;
    private bool create = true;
    private float lastTime = 0;
    private float timeToAct = 0;

    private readonly string[] urls = {
        "/GLB/TrunkSeparatedTextures/Trunk.glb",
        "/GLB/Lantern/Lantern.glb",
        "/GLB/DamagedHelmet/DamagedHelmet.glb",
        "/GLB/Trevor/Trevor.glb",
        "/GLB/vertex-anim.glb",
        "/GLB/draco-compressor.glb",
        "/GLB/cube_vertexcolor.glb",
        "/GLB/avatar-sitting/male/ch1_crowdV5.glb",
        "/GLB/avatar-sitting/female/ch2_crowdV5.glb",
        "/GLB/wings.glb",
    };

    private void Start()
    {
        CommonScriptableObjects.rendererState.Set(true);
        webRequestController = WebRequestController.Create();
        apkGltf = new AssetPromiseKeeper_GLTF();
        apkGltFast = new AssetPromiseKeeper_GLTFast_Instance();
        apkGltf.throttlingCounter.enabled = false;
    }

    private void Generate(string url)
    {
        AssetPromise_GLTF promise = new AssetPromise_GLTF(url, webRequestController);

        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-10, 10);
        pos.z = Random.Range(-10, 10);
        promise.settings.initialLocalPosition = pos;

        apkGltf.Keep(promise);
        gltfPromises.Add(promise);
    }

    private void GenerateGltFast(string url)
    {
        Debug.Log(url);
        AssetPromise_GLTFast_Instance promise2 = new AssetPromise_GLTFast_Instance(url, url, webRequestController);
        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-10, 10);
        pos.z = Random.Range(-10, 10);
        promise2.OverrideInitialPosition(pos);
        apkGltFast.Keep(promise2);
        gltFastPromises.Add(promise2);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
            CreateWithGltFast();

        if (Input.GetKeyUp(KeyCode.Z))
            CreateWithOldGltfImporter();
        else if (Input.GetKeyUp(KeyCode.X))
            Destroy();
        else if (Input.GetKeyUp(KeyCode.C))
            automatedMode = !automatedMode;

        if (!automatedMode || !((Time.time - lastTime) > timeToAct)) return;

        if (create)
            CreateWithOldGltfImporter();
        else
            Destroy();

        create = !create;
        lastTime = Time.time;
        timeToAct = Random.Range(0.05f, 0.15f);
    }
    private void Destroy()
    {
        if (gltfPromises.Count > 0)
        {
            var promiseToRemove = gltfPromises[Random.Range(0, gltfPromises.Count)];
            apkGltf.Forget(promiseToRemove);
            gltfPromises.Remove(promiseToRemove);
            PoolManager.i.Cleanup(true);
        }
        if (gltFastPromises.Count > 0)
        {
            var promiseToRemove = gltFastPromises[Random.Range(0, gltFastPromises.Count)];
            apkGltFast.Forget(promiseToRemove);
            gltFastPromises.Remove(promiseToRemove);
            PoolManager.i.Cleanup(true);
        }

    }
    private void CreateWithOldGltfImporter()
    {
        counter++;
        counter %= urls.Length;
        string finalUrl = TestAssetsUtils.GetPath() + urls[counter];
        Generate(finalUrl);
    }

    private void CreateWithGltFast()
    {
        counter++;
        counter %= urls.Length;
        string finalUrl = TestAssetsUtils.GetPath() + urls[counter];
        GenerateGltFast(finalUrl);
    }
}
