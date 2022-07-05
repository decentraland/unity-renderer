using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AvatarModifierAreaRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;
        private readonly AvatarModifierFactory modifierFactory;

        public AvatarModifierAreaRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            modifierFactory = new AvatarModifierFactory();
            
            factory.AddOrReplaceComponent(componentId, AvatarModifierAreaSerializer.Deserialize, () => new AvatarModifierAreaComponentHandler(Environment.i.platform.updateEventHandler, DataStore.i.player, modifierFactory));
            componentWriter.AddOrReplaceComponentSerializer<PBAvatarModifierArea>(componentId, AvatarModifierAreaSerializer.Serialize);

            this.factory = factory;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        public void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
            modifierFactory.Dispose();
        }
    }
}