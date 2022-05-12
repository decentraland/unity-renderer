using System;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSComponentHandler<ModelType>
    {
        /// <summary>
        /// Released when the component is ready
        /// </summary>
        event Action OnComponentReady;
        
        void OnComponentCreated(IParcelScene scene, IDCLEntity entity);
        void OnComponentRemoved(IParcelScene scene, IDCLEntity entity);
        void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ModelType model);
    }
}