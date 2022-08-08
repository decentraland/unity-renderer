using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSReadOnlyComponentData<T>
    {
        public IParcelScene scene { get; }
        public IDCLEntity entity { get; }
        public T model { get; }
        public IECSComponentHandler<T> handler { get; }
    }
}