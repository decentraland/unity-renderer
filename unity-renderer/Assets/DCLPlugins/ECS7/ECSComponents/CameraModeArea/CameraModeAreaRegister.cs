using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class CameraModeAreaRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public CameraModeAreaRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, CameraModeAreaSerializer.Deserialize, () => new CameraModeAreaComponentHandler(DataStore.i.ecs7));
            componentWriter.AddOrReplaceComponentSerializer<PBCameraModeArea>(componentId, CameraModeAreaSerializer.Serialize);

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