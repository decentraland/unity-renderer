using UnityEngine;
using DCL.Models;
using System.Collections.Generic;

public abstract class Attachable : MonoBehaviour
{         
    
    // From Gameobject instance id to its entity representation
    private readonly Dictionary<int, DecentralandEntity> entities = new Dictionary<int, DecentralandEntity>();

    public void AttachEntity(DecentralandEntity entity)
    {        
        int instanceId = entity.gameObject.GetInstanceID();
        entities.Add(instanceId, entity);
        entity.gameObject.transform.SetParent(transform, false);
        entity.OnRemoved += OnEntityRemoval;        
    }

    protected virtual void Setup() { }
    protected virtual void Cleanup() { }

    private void Awake()
    {
        // Setup
        Setup();

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
        // Stop listening to changes
        CommonScriptableObjects.playerWorldPosition.OnChange -= OnPlayerPositionChange;

        // Clean up
        Cleanup();
    }
}
