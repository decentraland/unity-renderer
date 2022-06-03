using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL.ECSRuntime;
using Google.Protobuf;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTextShapeComponent : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public ECSTextShapeComponent(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            ECSTextShapeComponentHandler handler = new ECSTextShapeComponentHandler(DataStore.i.ecs7, AssetPromiseKeeper_Font.i);
            factory.AddOrReplaceComponent(componentId, TextShapeSerialization.Deserialize, () => handler);
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
        
        internal byte[] SerializeComponent(PBTextShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }
    }
}