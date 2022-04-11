using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class ComponentData<T>
    {
        public IParcelScene scene { get; set; }
        public IDCLEntity entity { get; set; }
        public T model { get; set; }
    }
}