using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Blocker : MonoBehaviour
{

    [SerializeField] public List<TMP_Text> textFields;
    [SerializeField] public GameObject cubePivot;

    public Action blockerDone;
    private IParcelScene currentScene;
    private float centerOffset;

    private void Start()
    {
        centerOffset = ParcelSettings.PARCEL_SIZE / 2;
    }

    public void ResetBlocker(IParcelScene scene, Vector2Int pos, Transform parent)
    {
        Vector3 blockerPos = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(pos.x, pos.y));
        transform.SetParent(parent, false);
        transform.position = new Vector3(blockerPos.x + centerOffset, 8,blockerPos.z + centerOffset);
        transform.localScale = Vector3.one * 16;
        GetComponent<BoxCollider>().enabled = true;
        cubePivot.transform.localScale = Vector3.one;
        foreach (TMP_Text tmpText in textFields)
        {
            tmpText.text = $"Initializing";
        }
        currentScene = scene;
        currentScene.OnLoadingStateUpdated += LoadingStatusUpdated;
    }

    private void LoadingStatusUpdated(float arg1, int currentMissingObjects, int totalObjects, float arg4, float arg5)
    {
        float currentPercentage = 0;
        if (totalObjects > 0)
        {
            currentPercentage = (totalObjects - currentMissingObjects) / (float)totalObjects;
        }

        cubePivot.transform.localScale = new Vector3(1, 1 - currentPercentage, 1);
        foreach (TMP_Text tmpText in textFields)
        {
            tmpText.text = $"Percentage loaded {currentPercentage * 100}\nDownloading {totalObjects - currentMissingObjects} of {totalObjects}";
        }

        if (currentPercentage >= 1)
        {
            blockerDone?.Invoke();
            currentScene.OnLoadingStateUpdated -= LoadingStatusUpdated;
        }

        GetComponent<BoxCollider>().enabled = false;
        gameObject.SetActive(false);
    }

}

