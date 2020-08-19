using UnityEngine;
using DCL.Models;
using DCL.Controllers;
using System.Collections.Generic;

public class AvatarReference : MonoBehaviour
{         

    public static AvatarReference i { get; private set; }    
    // From Gameobject instance id to its entity representation
    private readonly Dictionary<int, DecentralandEntity> entities = new Dictionary<int, DecentralandEntity>();

    public void AttachEntity(DecentralandEntity entity)
    {        
        int instanceId = entity.gameObject.GetInstanceID();
        entities.Add(instanceId, entity);
        entity.gameObject.transform.SetParent(transform, false);
        entity.OnRemoved += OnEntityRemoval;
        Debug.Log("Attaching to avatar");
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
        
        // Listen to changes on the player position
        CommonScriptableObjects.playerWorldPosition.OnChange += OnPlayerPositionChange;        
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

    private void OnEntityRemoval(DecentralandEntity entity)
    {
        entities.Remove(entity.gameObject.GetInstanceID());
    }

    private void OnDestroy()
    {        
        CommonScriptableObjects.playerWorldPosition.OnChange -= OnPlayerPositionChange;
    }
}
