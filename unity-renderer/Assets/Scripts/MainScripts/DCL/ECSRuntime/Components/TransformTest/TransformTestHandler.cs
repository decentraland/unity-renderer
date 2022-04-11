using System;
using DCL.Controllers;
using DCL.ECSRuntime.Models;
using DCL.Models;

namespace DCL.ECSRuntime.TransformTest
{
    public class TransformTestHandler : IComponentHandler<ECSTransform>
    {
        void IComponentHandler<ECSTransform>.OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            throw new NotImplementedException();
        }
        void IComponentHandler<ECSTransform>.OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            throw new NotImplementedException();
        }
        void IComponentHandler<ECSTransform>.OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSTransform model)
        {
            throw new NotImplementedException();
        }
    }
}