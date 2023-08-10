using System;
using UnityEngine;

public class IKRuntimeConfiguration : MonoBehaviour
{
    [SerializeField] private Transform[] objectsToReparent;
    [SerializeField] private Transform parent;

    public void Start()
    {
        foreach (Transform tr in objectsToReparent)
        {
            //tr.SetParent(parent, true);
        }
    }
}
