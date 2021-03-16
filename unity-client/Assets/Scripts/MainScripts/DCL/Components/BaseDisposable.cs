using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseDisposable : IComponent
    {
        public virtual string componentName => GetType().Name;
        public string id;
        public IParcelScene scene { get; set; }

        public abstract int GetClassId();

        ComponentUpdateHandler updateHandler;
        public WaitForComponentUpdate yieldInstruction => updateHandler.yieldInstruction;
        public Coroutine routine => updateHandler.routine;
        public bool isRoutineRunning => updateHandler.isRoutineRunning;

        public event System.Action<DecentralandEntity> OnAttach;
        public event System.Action<DecentralandEntity> OnDetach;
        public event Action<BaseDisposable> OnAppliedChanges;

        public HashSet<DecentralandEntity> attachedEntities = new HashSet<DecentralandEntity>();

        protected BaseModel model;

        public virtual void UpdateFromJSON(string json)
        {
            UpdateFromModel(model.GetDataFromJSON(json));
        }

        public virtual void UpdateFromModel(BaseModel newModel)
        {
            model = newModel;
            updateHandler.ApplyChangesIfModified(model);
        }

        public BaseDisposable()
        {
            updateHandler = CreateUpdateHandler();
        }

        public virtual void RaiseOnAppliedChanges()
        {
            OnAppliedChanges?.Invoke(this);
        }

        public virtual void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
            {
                return;
            }

            System.Type thisType = overridenAttachedType != null ? overridenAttachedType : GetType();
            entity.AddSharedComponent(thisType, this);

            attachedEntities.Add(entity);

            entity.OnRemoved += OnEntityRemoved;

            OnAttach?.Invoke(entity);
        }

        private void OnEntityRemoved(DecentralandEntity entity)
        {
            DetachFrom(entity);
        }

        public virtual void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (!attachedEntities.Contains(entity)) return;

            entity.OnRemoved -= OnEntityRemoved;

            System.Type thisType = overridenAttachedType != null ? overridenAttachedType : GetType();
            entity.RemoveSharedComponent(thisType, false);

            attachedEntities.Remove(entity);

            OnDetach?.Invoke(entity);
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
        }

        public virtual BaseModel GetModel() => model;

        public abstract IEnumerator ApplyChanges(BaseModel model);

        public virtual ComponentUpdateHandler CreateUpdateHandler()
        {
            return new ComponentUpdateHandler(this);
        }

        public bool IsValid()
        {
            return true;
        }

        public void Cleanup()
        {
            if (isRoutineRunning)
            {
                CoroutineStarter.Stop(routine);
            }
        }

        public virtual void CallWhenReady(Action<BaseDisposable> callback)
        {
            //By default there's no initialization process and we call back as soon as we get the suscription
            callback.Invoke(this);
        }
    }
}