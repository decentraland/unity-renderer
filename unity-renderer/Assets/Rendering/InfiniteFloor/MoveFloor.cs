using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;
using DCL.Models;

public class MoveFloor : MonoBehaviour
{
    public LoadParcelScenesMessage.UnityParcelScene sceneData { get; protected set; }

    void OnEnable() 
    { 
        CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
    }

    void OnWorldReposition(UnityEngine.Vector3 current, UnityEngine.Vector3 previous)
    {
        //transform.position = previous + current;

        //UnityEngine.Vector3 currentSceneWorldPos = Utils.GridToWorldPosition(sceneData.basePosition.x, sceneData.basePosition.y);
        //UnityEngine.Vector3 newSceneUnityPos = PositionUtils.WorldToUnityPosition(currentSceneWorldPos);

        //transform.position = newSceneUnityPos;
    }

}


