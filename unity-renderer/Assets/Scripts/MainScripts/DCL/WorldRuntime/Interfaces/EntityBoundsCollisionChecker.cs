using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: Move this to a parcel/scene monobehaviour instead of on entities 
public class EntityBoundsCollisionChecker : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    private List<string> collidingScenes = new List<string>();
#endif
    
    public Action<string> OnParcelEntered;
    public Action<string> OnParcelExited;

    private void OnTriggerEnter(Collider other)
    {
        OnParcelEntered?.Invoke(other.name);
        
#if UNITY_EDITOR
        if(!collidingScenes.Contains(other.name))
            collidingScenes.Add(other.name);
#endif
    }

    private void OnTriggerExit(Collider other)
    {   
        OnParcelExited?.Invoke(other.name);
        
#if UNITY_EDITOR
        if(collidingScenes.Contains(other.name))
            collidingScenes.Remove(other.name);
#endif
    }
}
