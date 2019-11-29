using DCL;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;

public class APK_GLTF_InteractiveTest : MonoBehaviour
{
    ContentProvider_Dummy provider;
    AssetPromiseKeeper_GLTF keeper;

    List<AssetPromise_GLTF> promiseList = new List<AssetPromise_GLTF>();

    void Start()
    {
        GLTFSceneImporter.budgetPerFrameInMilliseconds = 4;

        provider = new ContentProvider_Dummy();
        keeper = new AssetPromiseKeeper_GLTF();
    }

    void Generate(string url)
    {
        AssetPromise_GLTF promise = new AssetPromise_GLTF(provider, url);

        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-10, 10);
        pos.z = Random.Range(-10, 10);
        promise.settings.initialLocalPosition = pos;

        keeper.Keep(promise);
        promiseList.Add(promise);
    }
    static int counter = 0;
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            counter++;
            counter %= 3;
            switch (counter)
            {
                case 0:
                    string url = Utils.GetTestsAssetsPath() + "/GLB/TrunkSeparatedTextures/Trunk.glb";
                    Generate(url);
                    break;
                case 1:
                    string url2 = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
                    Generate(url2);
                    break;
                case 2:
                    string url3 = Utils.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb";
                    Generate(url3);
                    break;
            }

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
