using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL.ECSRuntime;
using Google.Protobuf;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class UITransformRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public UITransformRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var handler = new UITransformComponentHandler(DataStore.i.ecs7.uiDataContainer);
            factory.AddOrReplaceComponent(componentId, UITransformSerializer.Deserialize, () => handler);
            componentWriter.AddOrReplaceComponentSerializer<PBUiTransform>(componentId, UITransformSerializer.Serialize);

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