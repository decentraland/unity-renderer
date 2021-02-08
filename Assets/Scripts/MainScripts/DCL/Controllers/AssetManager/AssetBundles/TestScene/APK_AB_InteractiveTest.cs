using DCL;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

public class APK_AB_InteractiveTest : MonoBehaviour
{
    AssetPromiseKeeper_AB_GameObject keeper;
    List<AssetPromise_AB_GameObject> promiseList = new List<AssetPromise_AB_GameObject>();

    public string baseUrl = "";
    public string fileToLoad = "QmZFSGh3KYXC4hnjjUvdnqE1owraaMcFXny8oL6ctHR47L";

    private Vector3 posOffset = Vector3.zero;

    void Start()
    {
        keeper = new AssetPromiseKeeper_AB_GameObject();
    }

    void Generate(string url, string hash)
    {
        AssetPromise_AB_GameObject promise = new AssetPromise_AB_GameObject(url, hash);

        Vector3 pos = posOffset;
        promise.settings.initialLocalPosition = pos;

        posOffset += Vector3.right * 10;

        keeper.Keep(promise);
        promiseList.Add(promise);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (string.IsNullOrEmpty(baseUrl))
                baseUrl = new System.Uri(Application.dataPath + "/../AssetBundles").AbsoluteUri + "/";

            Generate(baseUrl, fileToLoad);
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