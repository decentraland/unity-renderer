using UnityEngine;
using DCL.Models;
using DCL.Controllers;
using System.Collections.Generic;

public class PlayerReference : MonoBehaviour
{         

    public static PlayerReference i { get; private set; }    
    // From Gameobject instance id to its entity
    private readonly Dictionary<int, DecentralandEntity> entities = new Dictionary<int, DecentralandEntity>();

    public void AttachEntity(DecentralandEntity entity)
    {        
        int instanceId = entity.gameObject.GetInstanceID();
        entities.Add(instanceId, entity);
        entity.gameObject.transform.SetParent(transform, false);
        entity.OnRemoved += OnEntityRemoval;
        Debug.Log("Attaching to player");
    }

    private void Awake()
    {
        // Singleton setup
        if (i != null)
        {
            Destroy(gameObject);
            return;
        }

        i = this;

        // Listen to changes on the camera mode
        CommonScriptableObjects.cameraMode.OnChange += OnCameraModeChange;

        // Listen to changes on the player position
        CommonScriptableObjects.playerWorldPosition.OnChange += OnPlayerPositionChange;

        // Trigger the initial camera set
        OnCameraModeChange(CommonScriptableObjects.cameraMode, CommonScriptableObjects.cameraMode);
    }        

    private void OnPlayerPositionChange(Vector3 newPosition, Vector3 prev)
    {
        // When the position changes, perform boundaries check for all attached entities
        foreach (Transform child in transform) {
            int childId = child.gameObject.GetInstanceID();
            DecentralandEntity entity = entities[childId];
            DCL.SceneController.i.boundariesChecker.AddEntityToBeChecked(entity);        
        }        
    }

    private void UpdateForward(Vector3 newFordward, Vector3 prev)
    {       
        transform.forward = newFordward;
    }   

    private void OnCameraModeChange(CameraMode.ModeId newMode, CameraMode.ModeId prev)
    {
        if (newMode == CameraMode.ModeId.ThirdPerson)
        {
            CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
            transform.forward = transform.parent.forward;
        }
        else
        {
            CommonScriptableObjects.cameraForward.OnChange += UpdateForward;
        }
    }

    private void OnEntityRemoval(DecentralandEntity entity)
    {
        entities.Remove(entity.gameObject.GetInstanceID());
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.cameraMode.OnChange -= OnCameraModeChange;
        CommonScriptableObjects.cameraForward.OnChange -= UpdateForward;
        CommonScriptableObjects.playerWorldPosition.OnChange -= OnPlayerPositionChange;
    }
}
