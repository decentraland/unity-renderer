using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components {

  public abstract class BaseDisposable : IComponent {

    public Coroutine routine = null;
    public abstract string componentName { get; }

    public delegate void DisposableComponentEventDelegate(DecentralandEntity entity);
    public event DisposableComponentEventDelegate OnAttach;
    public event DisposableComponentEventDelegate OnDetach;

    private string oldSerialization = null;

    protected DCL.Controllers.ParcelScene scene { get; }
    protected HashSet<DecentralandEntity> attachedEntities = new HashSet<DecentralandEntity>();

    public void UpdateFromJSON(string json) {
      ApplyChangesIfModified(json);
    }

    public BaseDisposable(DCL.Controllers.ParcelScene scene) {
      this.scene = scene;
      ApplyChangesIfModified(oldSerialization ?? "{}");
    }

    private void ApplyChangesIfModified(string newSerialization) {
      if (newSerialization != oldSerialization) {
        //JsonUtility.FromJsonOverwrite(newSerialization, data);
        oldSerialization = newSerialization;

        // We use the scene start coroutine because we need to divide the computing resources fairly
        if (routine != null) {
          scene.StopCoroutine(routine);
          routine = null;
        }

        var enumerator = ApplyChanges(newSerialization);
        if (enumerator != null) {
          // we don't want to start coroutines if we have early finalization in IEnumerators
          // ergo, we return null without yielding any result
          routine = scene.StartCoroutine(enumerator);
        }
      }
    }

    public void AttachTo(DecentralandEntity entity) {
      if (!attachedEntities.Contains(entity)) {
        if (OnAttach != null) {
          OnAttach(entity);
        }

        attachedEntities.Add(entity);
      }
    }

    public void DetachFrom(DecentralandEntity entity) {
      if (attachedEntities.Contains(entity)) {
        if (OnDetach != null) {
          OnDetach(entity);
        }

        attachedEntities.Remove(entity);
      }
    }

    public void DetachFromEveryEntity() {
      DecentralandEntity[] attachedEntitiesArray = new DecentralandEntity[attachedEntities.Count];

      attachedEntities.CopyTo(attachedEntitiesArray);

      for (int i = 0; i < attachedEntitiesArray.Length; i++) {
        DetachFrom(attachedEntitiesArray[i]);
      }
    }

    public void Dispose() {
      DetachFromEveryEntity();

    }

    public abstract IEnumerator ApplyChanges(string newJson);
  }
}
