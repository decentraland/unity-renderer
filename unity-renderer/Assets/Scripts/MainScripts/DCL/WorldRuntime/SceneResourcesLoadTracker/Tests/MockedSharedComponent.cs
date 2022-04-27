using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;

namespace Tests
{
    public class MockedSharedComponent : ISharedComponent
    {
        private event Action<ISharedComponent> OnReady;
        public string id;

        string ISharedComponent.id => id;

        public MockedSharedComponent(string id)
        {
            this.id = id;
        }

        public void SetAsReady()
        {
            OnReady?.Invoke(this);
        }

        void ISharedComponent.CallWhenReady(Action<ISharedComponent> callback)
        {
            OnReady += callback;
        }

        IParcelScene IComponent.scene => throw new NotImplementedException();

        string IComponent.componentName => throw new NotImplementedException();

        void IComponent.UpdateFromJSON(string json)
        {
            throw new NotImplementedException();
        }

        void IComponent.RaiseOnAppliedChanges()
        {
            throw new NotImplementedException();
        }

        bool IComponent.IsValid()
        {
            throw new NotImplementedException();
        }

        BaseModel IComponent.GetModel()
        {
            throw new NotImplementedException();
        }

        int IComponent.GetClassId()
        {
            throw new NotImplementedException();
        }

        IEnumerator IComponent.ApplyChanges(BaseModel model)
        {
            throw new NotImplementedException();
        }

        void IComponent.UpdateFromModel(BaseModel model)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        event Action<IDCLEntity> ISharedComponent.OnAttach { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }

        event Action<IDCLEntity> ISharedComponent.OnDetach { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }

        void ISharedComponent.AttachTo(IDCLEntity entity, Type overridenAttachedType)
        {
            throw new NotImplementedException();
        }

        void ISharedComponent.DetachFrom(IDCLEntity entity, Type overridenAttachedType)
        {
            throw new NotImplementedException();
        }

        void ISharedComponent.DetachFromEveryEntity()
        {
            throw new NotImplementedException();
        }

        void ISharedComponent.Initialize(IParcelScene scene, string id)
        {
            throw new NotImplementedException();
        }

        HashSet<IDCLEntity> ISharedComponent.GetAttachedEntities()
        {
            throw new NotImplementedException();
        }
    }
}