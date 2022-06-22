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
        "/GLB/vertex-anim.glb"
    };

    void Start()
    {
        CommonScriptableObjects.rendererState.Set(true);
        webRequestController = WebRequestController.Create();
        keeper = new AssetPromiseKeeper_GLTF();
        keeper.throttlingCounter.enabled = false;
    }

    void Generate(string url)
    {
        AssetPromise_GLTF promise = new AssetPromise_GLTF(url, webRequestController);

        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-10, 10);
        pos.z = Random.Range(-10, 10);
        promise.settings.initialLocalPosition = pos;

        Vector3 pos2 = pos + Vector3.forward * 2;

        keeper.Keep(promise);
        promiseList.Add(promise);

        UniTask.Run(() => GenerateWithglTFast(url, pos2));
    }

    private async UniTaskVoid GenerateWithglTFast(string url, Vector3 position)
    {
        await UniTask.SwitchToMainThread();
        var gltf = new GltfImport();

        // Create a settings object and configure it accordingly
        var settings = new ImportSettings
        {
            generateMipMaps = false,
            anisotropicFilterLevel = 3,
            nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
        };

        // Load the glTF and pass along the settings
        var success = await gltf.Load(url, settings);

        if (success)
        {
            GameObject o = new GameObject("glTF");
            gltf.InstantiateMainScene(o.transform);
            o.transform.position = position;
        }
        else
        {
            Debug.LogError("Loading glTF failed!");
        }
    }

    void Update()
    {
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
    }
    private void Create()
    {
        counter++;
        counter %= urls.Length;
        string finalUrl = TestAssetsUtils.GetPath() + urls[counter];
        Generate(finalUrl);
    }
}