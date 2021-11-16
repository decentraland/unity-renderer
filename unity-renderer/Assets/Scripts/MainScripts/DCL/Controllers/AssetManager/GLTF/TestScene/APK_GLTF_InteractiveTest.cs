using DCL;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;

public class APK_GLTF_InteractiveTest : MonoBehaviour
{
    AssetPromiseKeeper_GLTF keeper;
    private WebRequestController webRequestController;
    List<AssetPromise_GLTF> promiseList = new List<AssetPromise_GLTF>();

    private int counter = 0;

    private string[] urls = new string[]
    {
        "/GLB/TrunkSeparatedTextures/Trunk.glb",
        "/GLB/Lantern/Lantern.glb",
        "/GLB/DamagedHelmet/DamagedHelmet.glb",
        "/GLB/Trevor/Trevor.glb"
    };

    void Start()
    {
        CommonScriptableObjects.rendererState.Set(true);
        GLTFThrottlingCounter.i.budgetPerFrameInMilliseconds = 1;
        webRequestController = WebRequestController.Create();
        keeper = new AssetPromiseKeeper_GLTF();
    }

    void Generate(string url)
    {
        AssetPromise_GLTF promise = new AssetPromise_GLTF(url, webRequestController);

        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-10, 10);
        pos.z = Random.Range(-10, 10);
        promise.settings.initialLocalPosition = pos;

        keeper.Keep(promise);
        promiseList.Add(promise);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            counter++;
            counter %= urls.Length;
            string finalUrl = TestAssetsUtils.GetPath() + urls[counter];
            Generate(finalUrl);
        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            if (promiseList.Count > 0)
            {
                var promiseToRemove = promiseList[Random.Range(0, promiseList.Count)];
                keeper.Forget(promiseToRemove);
                promiseList.Remove(promiseToRemove);
                PoolManager.i.Cleanup(true);
            }
        }
    }
}