using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public interface IComponent
    {
        string componentName { get; }
        void UpdateFromJSON(string json);
    }

    public abstract class UpdateableComponent : MonoBehaviour, IComponent
    {
        public virtual string componentName => GetType().Name;
        public abstract void UpdateFromJSON(string json);
    }

    public abstract class BaseComponent : UpdateableComponent
    {
        public Coroutine routine = null;

        [NonSerialized] public ParcelScene scene;
        [NonSerialized] public DecentralandEntity entity;

        private string oldSerialization = null;

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
            var enumerator = ApplyChanges(newJson);

            if (enumerator != null)
            {
                yield return enumerator;
            }

            if (entity != null && entity.OnComponentUpdated != null)
            {
                entity.OnComponentUpdated.Invoke(this);
            }
        }

        public abstract IEnumerator ApplyChanges(string newJson);
    }
}