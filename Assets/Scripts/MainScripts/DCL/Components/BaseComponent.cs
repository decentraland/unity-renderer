using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DCL.Models;
using DCL.Controllers;
using System;

namespace DCL.Components
{

    public interface IComponent
    {
        string componentName { get; }
        void UpdateFromJSON(string json);
    }

    public abstract class UpdateableComponent : MonoBehaviour, IComponent
    {
        public abstract string componentName { get; }
        public abstract void UpdateFromJSON(string json);
    }

    public abstract class BaseComponent : UpdateableComponent
    {
        [NonSerialized] public ParcelScene scene;
        [NonSerialized] public DecentralandEntity entity;

        private string oldSerialization = null;
        private Coroutine routine = null;

        public override void UpdateFromJSON(string json)
        {
            ApplyChangesIfModified(json);
        }

        void OnEnable()
        {
            ApplyChangesIfModified(oldSerialization ?? "{}");
        }

        private void ApplyChangesIfModified(string newSerialization)
        {
            if (newSerialization != oldSerialization)
            {
                oldSerialization = newSerialization;

                if (routine != null)
                {
                    StopCoroutine(routine);
                    routine = null;
                }

                var enumerator = UpdateComponent(newSerialization);
                if (enumerator != null)
                {
                    // we don't want to start coroutines if we have early finalization in IEnumerators
                    // ergo, we return null without yielding any result
                    routine = StartCoroutine(enumerator);
                }
            }
        }

        public virtual IEnumerator UpdateComponent(string newJson)
        {
            yield return ApplyChanges(newJson);

            if (entity != null && entity.OnComponentUpdated != null)
                entity.OnComponentUpdated.Invoke(this);
        }

        public abstract IEnumerator ApplyChanges(string newJson);
    }
}
