using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFloor : MonoBehaviour
{
    void OnEnable() 
    { 
        CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
    }

    void OnWorldReposition(UnityEngine.Vector3 current, UnityEngine.Vector3 previous)
    {
        transform.position -= current;
    }

}


