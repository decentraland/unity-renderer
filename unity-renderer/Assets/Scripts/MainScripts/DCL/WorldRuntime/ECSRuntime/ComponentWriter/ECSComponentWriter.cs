using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.CRDT;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public class ECSComponentWriter : IECSComponentWriter
    {
        private abstract class Serializer
        {
            private readonly Type targetType;

            protected Serializer(Type targetType)
            {
                this.targetType = targetType;
            }

            public bool CheckType(Type expectedType) =>
                targetType == expectedType;

            public abstract byte[] Serialize(object obj);
        }

        private class Serializer<T> : Serializer
        {
            private readonly Func<T, byte[]> serialize;

            public Serializer(Func<T, byte[]> serialize) : base(typeof(T))
            {
                this.serialize = serialize;
            }

            public override byte[] Serialize(object obj) =>
                serialize((T)obj);
        }

        public delegate void WriteComponent(int sceneNumber, long entityId, int componentId, byte[] data,
            int minTimeStamp, ECSComponentWriteType writeType, CrdtMessageType messageType);

        private readonly Dictionary<int, Serializer> serializers = new ();
        private WriteComponent writeComponent;

        public ECSComponentWriter(WriteComponent writeComponent)
        {
            this.writeComponent = writeComponent;
        }

        public void AddOrReplaceComponentSerializer<T>(int componentId, Func<T, byte[]> serializer)
        {
            serializers[componentId] = new Serializer<T>(serializer);
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

        public void PutComponent<T>(int sceneNumber, long entityId, int componentId, T model, int minTimeStamp, ECSComponentWriteType writeType)
        {
            PutComponent(typeof(T), sceneNumber, entityId, componentId, model, minTimeStamp, writeType);
        }

        public void PutComponent(Type componentType, int sceneNumber, long entityId, int componentId, object model, ECSComponentWriteType writeType)
        {
            PutComponent(componentType, sceneNumber, entityId, componentId, model, -1, writeType);
        }

        public void PutComponent(Type componentType, int sceneNumber, long entityId, int componentId, object model,
            int minTimeStamp, ECSComponentWriteType writeType)
        {
            if (!serializers.TryGetValue(componentId, out Serializer serializer))
            {
                Debug.LogError($"Trying to write component but no serializer was found for {componentId}");
                return;
            }

            if (serializer.CheckType(componentType))
            {
                writeComponent(sceneNumber, entityId, componentId, serializer.Serialize(model), minTimeStamp, writeType, CrdtMessageType.PUT_COMPONENT);
            }
            else
            {
                Debug.LogError($"Trying to write component but serializer for {componentId} does not match {componentType.Name}");
            }
        }

        public void RemoveComponent(int sceneNumber, long entityId, int componentId, ECSComponentWriteType writeType)
        {
            RemoveComponent(sceneNumber, entityId, componentId, -1, writeType);
        }

        public void RemoveComponent(int sceneNumber, long entityId, int componentId, int minTimeStamp, ECSComponentWriteType writeType)
        {
            writeComponent(sceneNumber, entityId, componentId, null, minTimeStamp, writeType, CrdtMessageType.DELETE_COMPONENT);
        }

        public void AppendComponent(Type componentType, int sceneNumber, long entityId, int componentId, object model, ECSComponentWriteType writeType)
        {
            if (!serializers.TryGetValue(componentId, out Serializer serializer))
            {
                Debug.LogError($"Trying to write an append component but no serializer was found for {componentId}");
                return;
            }

            if (serializer.CheckType(componentType))
            {
                writeComponent(sceneNumber, entityId, componentId, serializer.Serialize(model), -1, writeType, CrdtMessageType.APPEND_COMPONENT);
            }
            else
            {
                Debug.LogError($"Trying to write component but serializer for {componentId} does not match {componentType.Name}");
            }
        }

        public void AppendComponent<T>(int sceneNumber, long entityId, int componentId, T model, ECSComponentWriteType writeType)
        {
            AppendComponent(typeof(T), sceneNumber, entityId, componentId, model, writeType);
        }



        public void Dispose()
        {
            serializers.Clear();
            writeComponent = null;
        }
    }
}
