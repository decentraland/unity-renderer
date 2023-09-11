using DCL.Controllers;
using DCL.Models;

public interface IEcsComponentImplementation<ModelType>
{
    void SetModel(IParcelScene scene, long entityId, ModelType model);
    void Deserialize(IParcelScene scene, IDCLEntity entity, object message);
}

