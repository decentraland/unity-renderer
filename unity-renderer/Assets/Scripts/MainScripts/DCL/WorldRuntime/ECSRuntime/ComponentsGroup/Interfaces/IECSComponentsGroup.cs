using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSComponentsGroup
    {
        bool Match(IParcelScene scene, IDCLEntity entity);
        bool Match(IECSComponent component);
        void Add(IParcelScene scene, IDCLEntity entity);
        bool Remove(IDCLEntity entity);
    }
}