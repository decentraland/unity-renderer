using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public class ECSComponentWriter : IECSComponentWriter
    {
        public delegate void WriteComponent(string sceneId, long entityId, int componentId, byte[] data);

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

        public void PutComponent<T>(IParcelScene scene, IDCLEntity entity, int componentId, T model)
        {
            PutComponent(scene.sceneData.id, entity.entityId, componentId, model);
        }

        public void PutComponent<T>(string sceneId, long entityId, int componentId, T model)
        {
            if (!serializers.TryGetValue(componentId, out object serializer))
            {
                Debug.LogError($"Trying to write component but no serializer was found for {componentId}");
                return;
            }

            if (serializer is Func<T, byte[]> typedSerializer)
            {
                writeComponent(sceneId, entityId, componentId, typedSerializer(model));
            }
            else
            {
                Debug.LogError($"Trying to write component but serializer for {componentId} does not match {nameof(T)}");
            }
        }

        public void RemoveComponent(IParcelScene scene, IDCLEntity entity, int componentId)
        {
            RemoveComponent(scene.sceneData.id, entity.entityId, componentId);
        }

        public void RemoveComponent(string sceneId, long entityId, int componentId)
        {
            writeComponent(sceneId, entityId, componentId, null);
        }

        public void Dispose()
        {
            serializers.Clear();
            writeComponent = null;
        }
    }
}