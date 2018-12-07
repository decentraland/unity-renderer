using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components {
  public interface IDisposableComponent : IComponent {
    void AttachTo(DecentralandEntity entity);
    void DetachFrom(DecentralandEntity entity);
  }

  public abstract class BaseDisposable<T> : UnityEngine.Object, IDisposableComponent where T : new() {
    public T data = new T();
    public Coroutine routine = null;
    private string oldSerialization = null;
    protected DCL.Controllers.ParcelScene scene { get; }

    public abstract string componentName { get; }

    public void UpdateFromJSON(string json) {
      ApplyChangesIfModified(json);
    }

    public BaseDisposable(DCL.Controllers.ParcelScene scene) {
      this.scene = scene;
      ApplyChangesIfModified(oldSerialization ?? "{}");
    }

    private void ApplyChangesIfModified(string newSerialization) {
      if (newSerialization != oldSerialization) {
        JsonUtility.FromJsonOverwrite(newSerialization, data);
        oldSerialization = newSerialization;

        // We use the scene start coroutine because we need to divide the computing resources fairly
        var enumerator = ApplyChanges();
        if (enumerator != null) {
          if (routine != null) {
            scene.StopCoroutine(routine);
          }
          routine = scene.StartCoroutine(enumerator);
        }
      }
    }

    public abstract IEnumerator ApplyChanges();

    public abstract void AttachTo(DecentralandEntity entity);
    public abstract void DetachFrom(DecentralandEntity entity);
  }
}
