using DCL;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;

public class APK_GLTF_InteractiveTest : MonoBehaviour
{
    ContentProvider_Dummy provider;
    AssetPromiseKeeper_GLTF keeper;
    AssetLibrary_GLTF library;

    List<AssetPromise_GLTF> promiseList = new List<AssetPromise_GLTF>();

    void Start()
    {
        GLTFSceneImporter.budgetPerFrameInMilliseconds = 4;

        provider = new ContentProvider_Dummy();
        library = new AssetLibrary_GLTF();
        keeper = new AssetPromiseKeeper_GLTF(library);
    }

    void Generate()
    {
        string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
        AssetPromise_GLTF promise = new AssetPromise_GLTF(provider, url);

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
            Generate();
        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            if (promiseList.Count > 0)
            {
                var promiseToRemove = promiseList[Random.Range(0, promiseList.Count)];
                keeper.Forget(promiseToRemove);
                promiseList.Remove(promiseToRemove);
            }
        }

    }
}
