using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public interface IComponent : ICleanable
    {
        bool isRoutineRunning { get; }
        Coroutine routine { get; }
        string componentName { get; }
        void UpdateFromJSON(string json);
        void UpdateFromModel(BaseModel model);
        IEnumerator ApplyChanges(BaseModel model);
        void RaiseOnAppliedChanges();
        ComponentUpdateHandler CreateUpdateHandler();
        bool IsValid();
        BaseModel GetModel();
        int GetClassId();
    }

    /// <summary>
    /// Unity is unable to yield a coroutine while is already being yielded by another one.
    /// To fix that we wrap the routine in a CustomYieldInstruction.
    /// </summary>
    public class WaitForComponentUpdate : CleanableYieldInstruction
    {
        public IComponent component;

        public WaitForComponentUpdate(IComponent component)
        {
            this.component = component;
        }

        public override bool keepWaiting
        {
            get { return component.isRoutineRunning; }
        }

        public override void Cleanup()
        {
            component.Cleanup();
        }
    }

    public abstract class BaseComponent : MonoBehaviour, IComponent, IPoolLifecycleHandler, IPoolableObjectContainer
    {
        protected ComponentUpdateHandler updateHandler;
        public WaitForComponentUpdate yieldInstruction => updateHandler.yieldInstruction;
        public Coroutine routine => updateHandler.routine;
        public bool isRoutineRunning => updateHandler.isRoutineRunning;

        public IParcelScene scene;

        [NonSerialized]
        public DecentralandEntity entity;

        public PoolableObject poolableObject { get; set; }

        public string componentName => "BaseComponent";

        protected BaseModel model;

        public void RaiseOnAppliedChanges()
        {
        }

        public virtual void UpdateFromJSON(string json)
        {
            UpdateFromModel(model.GetDataFromJSON(json));
        }

        public virtual void UpdateFromModel(BaseModel newModel)
        {
            model = newModel;
            updateHandler.ApplyChangesIfModified(model);
        }

        public abstract IEnumerator ApplyChanges(BaseModel model);

        void OnEnable()
        {
            if (updateHandler == null)
                updateHandler = CreateUpdateHandler();
        }

        public virtual BaseModel GetModel() => model;

        public virtual ComponentUpdateHandler CreateUpdateHandler()
        {
            return new ComponentUpdateHandler(this);
        }

        public bool IsValid()
        {
            return this != null;
        }

        public virtual void Cleanup()
        {
            updateHandler.Cleanup();
        }

        public virtual void OnPoolRelease()
        {
            Cleanup();
        }

        public virtual void OnPoolGet()
        {
            if (updateHandler == null)
                updateHandler = CreateUpdateHandler();
        }

        public abstract int GetClassId();
    }
}