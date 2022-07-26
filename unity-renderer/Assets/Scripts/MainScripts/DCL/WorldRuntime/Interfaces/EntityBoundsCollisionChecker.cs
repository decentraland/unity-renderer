using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: Move this to a parcel/scene monobehaviour instead of on entities 
public class EntityBoundsCollisionChecker : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    List<string> collidingScenes = new List<string>();
#endif

    public Action<string> OnEnteredParcel;
    public Action<string> OnExitedParcel;
    
    /*private Transform entityTransform;

    public void SetEntityTransform(Transform newTransform)
    {
        entityTransform = newTransform;
        
        lastLocalEntityPosition = entityTransform.InverseTransformPoint(transform.position);
    }*/

    private void OnTriggerEnter(Collider other)
    {
        OnEnteredParcel?.Invoke(other.name);
        
#if UNITY_EDITOR
        if(!collidingScenes.Contains(other.name))
            collidingScenes.Add(other.name);
#endif
    }

    private void OnTriggerExit(Collider other)
    {   
        OnExitedParcel?.Invoke(other.name);
        
#if UNITY_EDITOR
        if(collidingScenes.Contains(other.name))
            collidingScenes.Remove(other.name);
#endif
    }

    /*private Vector3 lastLocalEntityPosition;
    // float movingSpeed;
    private void LateUpdate()
    {
        Vector3 newGroundWorldPos = entityTransform.TransformPoint(lastLocalEntityPosition);
        // movingSpeed = Vector3.Distance(newGroundWorldPos, transform.position);
        transform.position = newGroundWorldPos;
        
        // Vector3 newCharacterForward = entityTransform.TransformDirection(lastCharacterRotation);
        lastLocalEntityPosition = entityTransform.InverseTransformPoint(transform.position);
        // lastCharacterRotation = groundTransform.InverseTransformDirection(CommonScriptableObjects.characterForward.Get().Value);
        // lastGlobalCharacterRotation = CommonScriptableObjects.characterForward.Get().Value;
        
    }*/
}
