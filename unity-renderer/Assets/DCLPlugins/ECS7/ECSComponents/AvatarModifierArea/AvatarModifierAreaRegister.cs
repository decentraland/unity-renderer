using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AvatarModifierAreaRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AvatarModifierAreaRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, AvatarModifierAreaSerializer.Deserialize, () => new AvatarModifierAreaComponentHandler(DataStore.i.ecs7));
            componentWriter.AddOrReplaceComponentSerializer<PBAvatarModifierArea>(componentId, AvatarModifierAreaSerializer.Serialize);

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