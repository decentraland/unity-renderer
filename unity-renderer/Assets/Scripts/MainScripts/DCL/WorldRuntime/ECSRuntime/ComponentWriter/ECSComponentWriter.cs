using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public class ECSComponentWriter : IECSComponentWriter
    {
        public delegate void WriteComponent(int sceneNumber, long entityId, int componentId, byte[] data,
            long minTimeStamp, ECSComponentWriteType writeType);

        private readonly Dictionary<int, object> serializers = new Dictionary<int, object>();
        private WriteComponent writeComponent;

        public ECSComponentWriter(WriteComponent writeComponent)
        {
            this.writeComponent = writeComponent;
        }

        public void AddOrReplaceComponentSerializer<T>(int componentId, Func<T, byte[]> serializer)
        {
            serializers[componentId] = serializer;
        }

        public void RemoveComponentSerializer(int componentId)
        {
            serializers.Remove(componentId);
        }

        public void PutComponent<T>(IParcelScene scene, IDCLEntity entity, int componentId, T model, ECSComponentWriteType writeType)
        {
            PutComponent(scene.sceneData.sceneNumber, entity.entityId, componentId, model, -1, writeType);
        }

        public void PutComponent<T>(int sceneNumber, long entityId, int componentId, T model, ECSComponentWriteType writeType)
        {
            PutComponent(sceneNumber, entityId, componentId, model, -1, writeType);
        }

        public void PutComponent<T>(int sceneNumber, long entityId, int componentId, T model, long minTimeStamp, ECSComponentWriteType writeType)
        {
            if (!serializers.TryGetValue(componentId, out object serializer))
            {
                Debug.LogError($"Trying to write component but no serializer was found for {componentId}");
                return;
            }

            if (serializer is Func<T, byte[]> typedSerializer)
            {
                writeComponent(sceneNumber, entityId, componentId, typedSerializer(model), minTimeStamp, writeType);
            }
            else
            {
                Debug.LogError($"Trying to write component but serializer for {componentId} does not match {nameof(T)}");
            }
        }

        public void RemoveComponent(IParcelScene scene, IDCLEntity entity, int componentId, ECSComponentWriteType writeType)
        {
            RemoveComponent(scene.sceneData.sceneNumber, entity.entityId, componentId, writeType);
        }

        public void RemoveComponent(int sceneNumber, long entityId, int componentId, ECSComponentWriteType writeType)
        {
            RemoveComponent(sceneNumber, entityId, componentId, -1, writeType);
        }

        public void RemoveComponent(int sceneNumber, long entityId, int componentId, long minTimeStamp, ECSComponentWriteType writeType)
        {
            writeComponent(sceneNumber, entityId, componentId, null, minTimeStamp, writeType);
        }

        public void Dispose()
        {
            serializers.Clear();
            writeComponent = null;
        }
    }
}