using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public interface IEntityComponent : IComponent, ICleanable, IMonoBehaviour
    {
        IDCLEntity entity { get; }
        void Initialize(IParcelScene scene, IDCLEntity entity);
    }
}