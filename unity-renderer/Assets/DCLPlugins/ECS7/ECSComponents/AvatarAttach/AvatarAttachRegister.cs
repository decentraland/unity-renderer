using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AvatarAttachRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AvatarAttachRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, AvatarAttachSerializer.Deserialize, () => new AvatarAttachComponentHandler(Environment.i.platform.updateEventHandler));
            componentWriter.AddOrReplaceComponentSerializer<PBAvatarAttach>(componentId, AvatarAttachSerializer.Serialize);

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