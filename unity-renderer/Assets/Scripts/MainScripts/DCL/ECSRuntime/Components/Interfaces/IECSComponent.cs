using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSComponent
    {
        void Create(IDCLEntity entity);
        bool Remove(IDCLEntity entity);
        void Deserialize(IDCLEntity entity, object message);
        bool HasComponent(IDCLEntity entity);
    }
}