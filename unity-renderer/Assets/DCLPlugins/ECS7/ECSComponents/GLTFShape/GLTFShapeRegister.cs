using System;
using DCL.ECSRuntime;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class GLTFShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public GLTFShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, GLTFShapeSerializer.Deserialize, () => new GLTFShapeComponentHandler(DataStore.i.ecs7, CollidersManager.i));
            componentWriter.AddOrReplaceComponentSerializer<PBGLTFShape>(componentId, GLTFShapeSerializer.Serialize);

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