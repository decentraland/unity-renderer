using System;
using DCL;
using UnityEngine;

public class DataStoreBridge : MonoBehaviour
{
    public void UnpublishSceneResult(string json)
    {
        try
        {
            PublishSceneResultPayload payload = JsonUtility.FromJson<PublishSceneResultPayload>(json);
            DataStore.i.builderInWorld.unpublishSceneResult.Set(payload, notifyEvent: true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}