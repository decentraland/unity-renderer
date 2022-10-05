using System;
using DCL;
using UnityEngine;

public class AbDetectorBridge : MonoBehaviour
{
    [Serializable]
    class DetectABsPayload
    {
        public bool isOn;
        public bool forCurrentScene;
    }
    

    public void DetectABs(string payload)
    {
        var data = JsonUtility.FromJson<DetectABsPayload>(payload);
        if (data.forCurrentScene)
        {
            DataStore.i.debugConfig.showSceneABDetectionLayer.Set(data.isOn, notifyEvent:true);
        }
        else
        {
            DataStore.i.debugConfig.showGlobalABDetectionLayer.Set(data.isOn, notifyEvent:true);
        }
    }
}