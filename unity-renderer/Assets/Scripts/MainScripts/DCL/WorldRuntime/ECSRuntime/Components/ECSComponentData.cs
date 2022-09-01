using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class ECSComponentData<T> : IECSReadOnlyComponentData<T>
    {
        public IParcelScene scene { get; set; }
        public IDCLEntity entity { get; set; }
        public T model { get; set; }
        public IECSComponentHandler<T> handler { get; set; }
    }
}