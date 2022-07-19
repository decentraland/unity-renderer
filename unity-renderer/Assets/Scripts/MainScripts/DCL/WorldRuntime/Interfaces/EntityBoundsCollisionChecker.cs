using System;
using UnityEngine;

// TODO: Move this to a parcel/scene monobehaviour instead of on entities 
public class EntityBoundsCollisionChecker : MonoBehaviour
{
    public Action<string> OnParcelEntered;
    public Action<string> OnParcelExited;

    private void OnTriggerEnter(Collider other)
    {
        OnParcelEntered?.Invoke(other.name);
    }

    private void OnTriggerExit(Collider other)
    {   
        OnParcelExited?.Invoke(other.name);
    }
}
