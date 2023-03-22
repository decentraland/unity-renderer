using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSComponentsGroup
    {
        bool MatchEntity(IParcelScene scene, IDCLEntity entity);
        bool ShouldAddOnComponentAdd(IECSComponent component);
        bool ShouldRemoveOnComponentRemove(IECSComponent component);
        bool ShouldRemoveOnComponentAdd(IECSComponent component);
        bool ShouldAddOnComponentRemove(IECSComponent component);
        void Add(IParcelScene scene, IDCLEntity entity);
        bool Remove(IDCLEntity entity);
    }
}
