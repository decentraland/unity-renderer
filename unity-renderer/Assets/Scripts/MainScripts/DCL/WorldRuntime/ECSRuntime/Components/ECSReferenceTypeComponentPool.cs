using DCL.ECS7.ComponentWrapper.Generic;

namespace DCL.ECSRuntime
{
    public class ECSReferenceTypeIecsComponentPool<ModelType> : IECSComponentPool<ModelType> where ModelType : class
    {
        private readonly WrappedComponentPool<IWrappedComponent<ModelType>> internalPool;

        public ECSReferenceTypeIecsComponentPool(WrappedComponentPool<IWrappedComponent<ModelType>> internalPool)
        {
            this.internalPool = internalPool;
        }

        public ModelType Get()
        {
            return (ModelType)internalPool.Get().WrappedComponentBase;
        }

        public void Release(ModelType item)
        {
            internalPool.Release(item as PooledWrappedComponent<IWrappedComponent<ModelType>>);
        }
    }
}
