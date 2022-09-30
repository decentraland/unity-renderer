using System;
using DCL;
using UnityEngine;

public class LoadingBridge : MonoBehaviour
{
    public void SetLoadingScreen(string jsonMessage) { DCL.Environment.i.world.teleportController.SetLoadingPayload(jsonMessage); }
    
}