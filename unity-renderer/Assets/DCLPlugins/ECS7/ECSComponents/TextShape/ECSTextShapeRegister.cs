using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL.ECSRuntime;
using Google.Protobuf;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTextShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public ECSTextShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, TextShapeSerialization.Deserialize, () => new ECSTextShapeComponentHandler(DataStore.i.ecs7, AssetPromiseKeeper_Font.i));
            componentWriter.AddOrReplaceComponentSerializer<PBTextShape>(componentId, TextShapeSerialization.Serialize);

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