using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSComponentHandler<in ModelType>
    {
        void OnComponentCreated(IParcelScene scene, IDCLEntity entity);
        void OnComponentRemoved(IParcelScene scene, IDCLEntity entity);
        void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ModelType model);
    }
}
