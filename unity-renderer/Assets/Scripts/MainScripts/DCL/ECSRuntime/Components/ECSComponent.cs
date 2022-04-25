using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public class ECSComponent<ModelType> : IECSComponent
    {
        internal Dictionary<string, ECSComponentData<ModelType>> entities = new Dictionary<string, ECSComponentData<ModelType>>();
        internal Dictionary<string, IECSComponentHandler<ModelType>> handlers = new Dictionary<string, IECSComponentHandler<ModelType>>();

        private readonly Func<IECSComponentHandler<ModelType>> handlerBuilder;
        private readonly Func<object, ModelType> deserializer;
        private readonly IParcelScene scene;

        public ECSComponent(IParcelScene scene, Func<object, ModelType> deserializer, Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            this.scene = scene;
            this.deserializer = deserializer;
            this.handlerBuilder = handlerBuilder;
        }

        public void Create(IDCLEntity entity)
        {
            var entityId = entity.entityId;

            if (entities.ContainsKey(entityId))
            {
                Debug.LogError($"entity {entityId} already contains component {typeof(ModelType)}", entity.gameObject);
                return;
            }

            entities[entityId] = new ECSComponentData<ModelType>()
            {
                entity = entity,
                model = default,
                scene = scene
            };

            var handler = handlerBuilder?.Invoke();

            if (handler != null)
            {
                handlers[entityId] = handler;
                handler.OnComponentCreated(scene, entity);
            }
        }

        public bool Remove(IDCLEntity entity)
        {
            var entityId = entity.entityId;
            if (handlers.TryGetValue(entityId, out IECSComponentHandler<ModelType> handler))
            {
                handler.OnComponentRemoved(scene, entity);
            }
            handlers.Remove(entityId);
            return entities.Remove(entityId);
        }

        public void SetModel(IDCLEntity entity, ModelType model)
        {
            var entityId = entity.entityId;
            if (entities.TryGetValue(entity.entityId, out ECSComponentData<ModelType> data))
            {
                data.model = model;
            }
            else
            {
                Debug.LogError($"trying to update model but entity {entityId} does not contains component {typeof(ModelType)}",
                    entity.gameObject);
            }

            if (handlers.TryGetValue(entityId, out IECSComponentHandler<ModelType> handler))
            {
                handler.OnComponentModelUpdated(scene, entity, model);
            }
        }

        public void Deserialize(IDCLEntity entity, object message)
        {
            SetModel(entity, deserializer.Invoke(message));
        }

        public bool HasComponent(IDCLEntity entity)
        {
            return entities.ContainsKey(entity.entityId);
        }

        public ECSComponentData<ModelType> Get(IDCLEntity entity)
        {
            if (entities.TryGetValue(entity.entityId, out ECSComponentData<ModelType> data))
            {
                return data;
            }
            return null;
        }

        public IEnumerator<ECSComponentData<ModelType>> Get()
        {
            using (var iterator = entities.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    yield return iterator.Current.Value;
                }
            }
        }
    }
}