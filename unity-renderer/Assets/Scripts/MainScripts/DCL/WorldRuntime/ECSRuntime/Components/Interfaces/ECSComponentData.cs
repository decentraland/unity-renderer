using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public readonly struct ECSComponentData<T>
    {
        public readonly IParcelScene scene;
        public readonly IDCLEntity entity;
        public readonly T model;
        public readonly IECSComponentHandler<T> handler;

        public ECSComponentData(IParcelScene scene, IDCLEntity entity, T model, IECSComponentHandler<T> handler)
        {
            this.scene = scene;
            this.entity = entity;
            this.model = model;
            this.handler = handler;
        }

        public ECSComponentData<T> With(T newModel)
        {
            return new ECSComponentData<T>(scene, entity, newModel, handler);
        }
    }
}
