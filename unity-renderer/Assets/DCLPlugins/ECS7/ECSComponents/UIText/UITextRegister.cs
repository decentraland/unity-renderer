using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL.ECSRuntime;
using Google.Protobuf;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class UITextRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public UITextRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var handler = new UITextComponentHandler(DataStore.i.ecs7.uiDataContainer);
            factory.AddOrReplaceComponent(componentId, UITextSerializer.Deserialize, () => handler);
            componentWriter.AddOrReplaceComponentSerializer<PBUiText>(componentId, UITextSerializer.Serialize);

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