using DCL;
using DCL.Helpers;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGLTF;
using GLTFast;

public class APK_GLTF_InteractiveTest : MonoBehaviour
{
    AssetPromiseKeeper_GLTF keeper;
    private WebRequestController webRequestController;
    List<AssetPromise_GLTF> promiseList = new List<AssetPromise_GLTF>();
    List<AssetPromise_GLTFast_GameObject> promiseList2 = new List<AssetPromise_GLTFast_GameObject>();

    private int counter = 0;
    private bool automatedMode = false;
    private bool create = true;
    private float lastTime = 0;
    private float timeToAct = 0;

    private string[] urls = new string[]
    {
        "/GLB/TrunkSeparatedTextures/Trunk.glb",
        "/GLB/Lantern/Lantern.glb",
        "/GLB/DamagedHelmet/DamagedHelmet.glb",
        "/GLB/Trevor/Trevor.glb",
        "/GLB/vertex-anim.glb",
        "/GLB/draco-compressor.glb",
        "/GLB/cube_vertexcolor.glb",
    };
    private AssetPromiseKeeper_GLTFast_GameObject keeper2;

    void Start()
    {
        CommonScriptableObjects.rendererState.Set(true);
        webRequestController = WebRequestController.Create();
        keeper = new AssetPromiseKeeper_GLTF();
        keeper2 = new AssetPromiseKeeper_GLTFast_GameObject();
        keeper.throttlingCounter.enabled = false;
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

    void GenerateGLTFast(string url)
    {
        Debug.Log(url);
        AssetPromise_GLTFast_GameObject promise2 = new AssetPromise_GLTFast_GameObject(url, url, webRequestController);
        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-10, 10);
        pos.z = Random.Range(-10, 10);
        promise2.settings.initialLocalPosition = pos;
        keeper2.Keep(promise2);
        promiseList2.Add(promise2);
    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            Create2();
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            Create();
        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            Destroy();
        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            automatedMode = !automatedMode;
        }

        if (automatedMode && (Time.time - lastTime) > timeToAct)
        {
            if (create)
            {
                Create();
            }
            else
            {
                Destroy();
            }

            create = !create;
            lastTime = Time.time;
            timeToAct = Random.Range(0.05f, 0.15f);
        }
    }
    private void Destroy()
    {
        if (promiseList.Count > 0)
        {
            var promiseToRemove = promiseList[Random.Range(0, promiseList.Count)];
            keeper.Forget(promiseToRemove);
            promiseList.Remove(promiseToRemove);
            PoolManager.i.Cleanup(true);
        }
        if (promiseList2.Count > 0)
        {
            var promiseToRemove = promiseList2[Random.Range(0, promiseList2.Count)];
            keeper2.Forget(promiseToRemove);
            promiseList2.Remove(promiseToRemove);
            PoolManager.i.Cleanup(true);
        }
        
    }
    private void Create()
    {
        counter++;
        counter %= urls.Length;
        string finalUrl = TestAssetsUtils.GetPath() + urls[counter];
        Generate(finalUrl);
    }
    
    private void Create2()
    {
        counter++;
        counter %= urls.Length;
        string finalUrl = TestAssetsUtils.GetPath() + urls[counter];
        
        GenerateGLTFast(finalUrl);
    }
}