using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AnimatorRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AnimatorRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, AnimatorSerializer.Deserialize, () => new AnimatorComponentHandler(DataStore.i.ecs7));
            componentWriter.AddOrReplaceComponentSerializer<PBAnimator>(componentId, AnimatorSerializer.Serialize);

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