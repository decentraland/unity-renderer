using System;
using DCL.FPSDisplay;
using TMPro;
using UnityEngine;

public class DebugValue : MonoBehaviour
{

    [SerializeField] internal TMP_Text textValue;
    
    public DebugValueEnum debugValueEnum;

    public void SetValue(string newValue)
    {
        textValue.text = newValue;
    }
    
}

public enum DebugValueEnum
{
    Skybox_Config, Skybox_Duration, Skybox_GameTime, Skybox_UTCTime, General_Network, General_Realm, General_NearbyPlayers, FPS, FPS_HiccupsInTheLast1000, FPS_HiccupsLoss, FPS_BadFramesPercentiles,
    Scene_ProcessedMessages, Scene_PendingOnQueue, Scene_Poly, Scene_Textures, Scene_Materials, Scene_Entities, Scene_Meshes, Scene_Bodies, Scene_Components, Scene_Name,
    Memory_Used_JS_Heap_Size, Memory_Limit_JS_Heap_Size, Memory_Total_JS_Heap_Size,
    Memory_Total_Allocated, Memory_Reserved_Ram, Memory_Mono, Memory_Graphics_Card
}

