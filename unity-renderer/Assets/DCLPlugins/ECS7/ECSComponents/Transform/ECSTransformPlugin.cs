using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class ECSTransformPlugin : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly int componentId;

        public ECSTransformPlugin(int componentId, ECSComponentsFactory factory)
        {
            ECSTransformHandler handler = new ECSTransformHandler();
            factory.AddOrReplaceComponent(componentId, ECSTransformSerialization.Deserialize, () => handler);
            this.factory = factory;
            this.componentId = componentId;
        }

        public void Dispose()
        {
            factory.RemoveComponent(componentId);
        }
    }
}