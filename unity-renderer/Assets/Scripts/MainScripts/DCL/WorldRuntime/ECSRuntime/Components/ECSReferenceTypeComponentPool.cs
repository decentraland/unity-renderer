using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;

namespace DCL.ECSRuntime
{
    public class ECSReferenceTypeIECSComponentPool<ModelType> : IECSComponentPool<ModelType> where ModelType : class
    {
        private readonly WrappedComponentPool<IWrappedComponent<ModelType>> internalPool;

        public ECSReferenceTypeIECSComponentPool(WrappedComponentPool<IWrappedComponent<ModelType>> internalPool)
        {
            this.internalPool = internalPool;
        }

        public ModelType Get()
        {
            PooledWrappedComponent<IWrappedComponent<ModelType>> pooledWrappedComponent = internalPool.Get();
            ModelType wrappedComponent = pooledWrappedComponent.WrappedComponent.Model;

            return wrappedComponent;
        }

        public void Release(ModelType item)
        {
            internalPool.Release(item as PooledWrappedComponent<IWrappedComponent<ModelType>>);
        }
    }
}
