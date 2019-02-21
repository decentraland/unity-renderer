using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{

    public abstract class BaseDisposable : IComponent
    {
        public Coroutine routine = null;
        public abstract string componentName { get; }
        public string id;

        public event System.Action<DecentralandEntity> OnAttach;
        public event System.Action<DecentralandEntity> OnDetach;


        private string oldSerialization = null;

        protected DCL.Controllers.ParcelScene scene { get; }
        public HashSet<DecentralandEntity> attachedEntities = new HashSet<DecentralandEntity>();

        public void UpdateFromJSON(string json)
        {
            ApplyChangesIfModified(json);
        }

        public BaseDisposable(DCL.Controllers.ParcelScene scene)
        {
            this.scene = scene;
            ApplyChangesIfModified(oldSerialization ?? "{}");
        }

        private void ApplyChangesIfModified(string newSerialization)
        {
            if (newSerialization == oldSerialization)
                return;

            oldSerialization = newSerialization;

            // We use the scene start coroutine because we need to divide the computing resources fairly
            if (routine != null)
            {
                scene.StopCoroutine(routine);
                routine = null;
            }

            var enumerator = ApplyChanges(newSerialization);
            if (enumerator != null)
            {
                // we don't want to start coroutines if we have early finalization in IEnumerators
                // ergo, we return null without yielding any result
                routine = scene.StartCoroutine(enumerator);
            }
        }

        public virtual void AttachTo(DecentralandEntity entity)
        {
            if (attachedEntities.Contains(entity))
                return;

            if (OnAttach != null)
                OnAttach.Invoke(entity);

            attachedEntities.Add(entity);
            entity.OnRemoved += OnEntityRemoved;
        }

        private void OnEntityRemoved(DecentralandEntity entity)
        {
            DetachFrom(entity);
        }

        public virtual void DetachFrom(DecentralandEntity entity)
        {
            if (!attachedEntities.Contains(entity))
                return;

            if (OnDetach != null)
                OnDetach.Invoke(entity);

            entity.OnRemoved -= OnEntityRemoved;
            attachedEntities.Remove(entity);
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

        public abstract IEnumerator ApplyChanges(string newJson);
    }
}
