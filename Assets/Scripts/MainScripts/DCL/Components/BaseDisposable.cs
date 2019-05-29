using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseDisposable : IComponent
    {
        public Coroutine routine = null;
        public virtual string componentName => GetType().Name;
        public string id;

        public event System.Action<DecentralandEntity> OnAttach;
        public event System.Action<DecentralandEntity> OnDetach;
        public event Action<BaseDisposable> OnAppliedChanges;

        private string oldSerialization = null;

        public DCL.Controllers.ParcelScene scene { get; }
        public HashSet<DecentralandEntity> attachedEntities = new HashSet<DecentralandEntity>();

        public void UpdateFromJSON(string json)
        {
            ApplyChangesIfModified(json);
        }

        public BaseDisposable(DCL.Controllers.ParcelScene scene)
        {
            this.scene = scene;
        }

        public void RaiseOnAppliedChanges()
        {
            if (OnAppliedChanges != null)
            {
                OnAppliedChanges.Invoke(this);
            }
        }

        private void ApplyChangesIfModified(string newSerialization)
        {
            if (newSerialization == oldSerialization)
            {
                return;
            }

            oldSerialization = newSerialization;

            // We use the scene start coroutine because we need to divide the computing resources fairly
            if (routine != null)
            {
                scene.StopCoroutine(routine);
                routine = null;
            }

            var enumerator = ApplyChangesWrapper(newSerialization);
            if (enumerator != null)
            {
                // we don't want to start coroutines if we have early finalization in IEnumerators
                // ergo, we return null without yielding any result
                routine = scene.StartCoroutine(enumerator);
            }
        }

        public virtual void AttachTo(DecentralandEntity entity, Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
            {
                return;
            }

            Type thisType = overridenAttachedType != null ? overridenAttachedType : GetType();
            entity.AddSharedComponent(thisType, this);

            if (OnAttach != null)
            {
                OnAttach.Invoke(entity);
            }

            attachedEntities.Add(entity);

            entity.OnRemoved += OnEntityRemoved;
        }

        private void OnEntityRemoved(DecentralandEntity entity)
        {
            DetachFrom(entity);
        }

        public virtual void DetachFrom(DecentralandEntity entity, Type overridenAttachedType = null)
        {
            if (!attachedEntities.Contains(entity))
            {
                return;
            }

            entity.OnRemoved -= OnEntityRemoved;

            Type thisType = overridenAttachedType != null ? overridenAttachedType : GetType();
            entity.RemoveSharedComponent(thisType, false);

            attachedEntities.Remove(entity);

            if (OnDetach != null)
            {
                OnDetach.Invoke(entity);
            }
        }

        public void DetachFromEveryEntity()
        {
            DecentralandEntity[] attachedEntitiesArray = new DecentralandEntity[attachedEntities.Count];

            attachedEntities.CopyTo(attachedEntitiesArray);

            for (int i = 0; i < attachedEntitiesArray.Length; i++)
            {
                DetachFrom(attachedEntitiesArray[i]);
            }
        }

        public virtual void Dispose()
        {
            DetachFromEveryEntity();
            Resources.UnloadUnusedAssets(); //NOTE(Brian): This will ensure assets are freed correctly.
        }

        public virtual IEnumerator ApplyChangesWrapper(string newJson)
        {
            var enumerator = ApplyChanges(newJson);

            if (enumerator != null)
            {
                yield return enumerator;
            }

            RaiseOnAppliedChanges();
        }

        public abstract IEnumerator ApplyChanges(string newJson);
    }
}