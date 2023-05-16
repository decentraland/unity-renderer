using DCL.Controllers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public abstract class BaseComponent : MonoBehaviour, IEntityComponent, IDelayedComponent, IPoolLifecycleHandler, IPoolableObjectContainer
    {
        protected ComponentUpdateHandler updateHandler;
        public CustomYieldInstruction yieldInstruction => updateHandler.yieldInstruction;
        public Coroutine routine => updateHandler.routine;
        public bool isRoutineRunning => updateHandler.isRoutineRunning;

        public IParcelScene scene { get; set; }

        public IDCLEntity entity { get; set; }

        public IPoolableObject poolableObject { get; set; }

        string IComponent.componentName => componentName;

        public abstract string componentName { get; }

        protected BaseModel model;

        public void RaiseOnAppliedChanges() { }

        public virtual void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            this.entity = entity;

            if (transform.parent != entity.gameObject.transform)
                transform.SetParent(entity.gameObject.transform, false);
        }

        public virtual void UpdateFromJSON(string json) { UpdateFromModel(model.GetDataFromJSON(json)); }

        public virtual void UpdateFromPb(ComponentBodyPayload payload)
        {
            UpdateFromModel(model.GetDataFromPb(payload));
        }

        public virtual void UpdateFromModel(BaseModel newModel)
        {
            model = newModel;
            updateHandler.ApplyChangesIfModified(model);
        }

        public abstract IEnumerator ApplyChanges(BaseModel model);

        protected void OnEnable()
        {
            if (updateHandler == null)
                updateHandler = CreateUpdateHandler();
        }

        public virtual BaseModel GetModel() => model;

        protected virtual ComponentUpdateHandler CreateUpdateHandler() { return new ComponentUpdateHandler(this); }

        public bool IsValid() { return this != null; }

        public virtual void Cleanup()
        {
            updateHandler?.Cleanup();
        }

        public virtual void OnPoolRelease() { Cleanup(); }

        public virtual void OnPoolGet()
        {
            if (updateHandler == null)
                updateHandler = CreateUpdateHandler();
        }

        public abstract int GetClassId();

        public Transform GetTransform() => transform;
    }
}
