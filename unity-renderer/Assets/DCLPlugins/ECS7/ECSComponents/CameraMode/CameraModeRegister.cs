using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class CameraModeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public CameraModeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            // since this component travels only one way (from renderer to scene) we don't need a handler
            factory.AddOrReplaceComponent(componentId, CameraModeSerializer.Deserialize, () => null);
            componentWriter.AddOrReplaceComponentSerializer<PBCameraMode>(componentId, CameraModeSerializer.Serialize);

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