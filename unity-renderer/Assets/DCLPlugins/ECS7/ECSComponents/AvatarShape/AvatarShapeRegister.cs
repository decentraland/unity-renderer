using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AvatarShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AvatarShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, AvatarShapeSerializer.Deserialize, () => new AvatarShapeComponentHandler(DataStore.i.ecs7));
            componentWriter.AddOrReplaceComponentSerializer<PBAvatarShape>(componentId, AvatarShapeSerializer.Serialize);

            this.factory = factory;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        public void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
        }
    }
}