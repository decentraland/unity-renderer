using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class ECSComponent<ModelType> : IECSComponent
    {
        internal Dictionary<string, ComponentData<ModelType>> entities = new Dictionary<string, ComponentData<ModelType>>();
        internal Dictionary<string, IComponentHandler<ModelType>> handlers = new Dictionary<string, IComponentHandler<ModelType>>();

        private readonly Func<IComponentHandler<ModelType>> handlerBuilder;
        private readonly Func<object, ModelType> deserializer;
        private readonly IParcelScene scene;

        public ECSComponent(IParcelScene scene, Func<object, ModelType> deserializer, Func<IComponentHandler<ModelType>> handlerBuilder)
        {
            this.scene = scene;
            this.deserializer = deserializer;
            this.handlerBuilder = handlerBuilder;
        }

        public void Create(IDCLEntity entity)
        {
            var entityId = entity.entityId;
            entities[entityId] = new ComponentData<ModelType>()
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
            if (handlers.TryGetValue(entityId, out IComponentHandler<ModelType> handler))
            {
                handler.OnComponentRemoved(scene, entity);
            }
            handlers.Remove(entityId);
            return entities.Remove(entityId);
        }

        public void SetModel(IDCLEntity entity, ModelType model)
        {
            var entityId = entity.entityId;
            if (entities.TryGetValue(entity.entityId, out ComponentData<ModelType> data))
            {
                data.model = model;
            }
            if (handlers.TryGetValue(entityId, out IComponentHandler<ModelType> handler))
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

        public ComponentData<ModelType> Get(IDCLEntity entity)
        {
            if (entities.TryGetValue(entity.entityId, out ComponentData<ModelType> data))
            {
                return data;
            }
            return null;
        }

        public IEnumerator<ComponentData<ModelType>> Get()
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