using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DCL.Models;

namespace DCL.Components {

  public interface IComponent {
    string componentName { get; }
    void UpdateFromJSON(string json);
  }

  public abstract class UpdateableComponent : MonoBehaviour, IComponent {
    public abstract string componentName { get; }
    public abstract void UpdateFromJSON(string json);
  }

  public abstract class BaseComponent<T> : UpdateableComponent where T : new() {
    public T data = new T();
    private string oldSerialization = null;
    private Coroutine routine = null;

    public override void UpdateFromJSON(string json) {
      ApplyChangesIfModified(json);
    }

    void OnEnable() {
      ApplyChangesIfModified(oldSerialization ?? "{}");
    }

    private void ApplyChangesIfModified(string newSerialization) {
      if (newSerialization != oldSerialization) {
        JsonUtility.FromJsonOverwrite(newSerialization, data);
        oldSerialization = newSerialization;
        var enumerator = ApplyChanges();
        if (enumerator != null) {
          if (routine != null) {
            StopCoroutine(routine);
          }
          routine = StartCoroutine(enumerator);
        }
      }
    }

    public abstract IEnumerator ApplyChanges();
  }
}
