using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public class ECSComponent<ModelType> : IECSComponent
    {
        internal Dictionary<long, ECSComponentData<ModelType>> entities = new Dictionary<long, ECSComponentData<ModelType>>();
        internal Dictionary<long, IECSComponentHandler<ModelType>> handlers = new Dictionary<long, IECSComponentHandler<ModelType>>();

        private readonly Func<IECSComponentHandler<ModelType>> handlerBuilder;
        private readonly Func<object, ModelType> deserializer;
        private readonly IParcelScene scene;

        public ECSComponent(IParcelScene scene, Func<object, ModelType> deserializer, Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            this.scene = scene;
            this.deserializer = deserializer;
            this.handlerBuilder = handlerBuilder;
        }

        /// <summary>
        /// creates and add component to an entity
        /// </summary>
        /// <param name="entity">target entity</param>
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

        /// <summary>
        /// remove component from entity
        /// </summary>
        /// <param name="entity">target entity</param>
        /// <returns>true if component removed successfully, false if entity didn't contain component</returns>
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

        /// <summary>
        /// set component model for entity
        /// </summary>
        /// <param name="entity">target entity</param>
        /// <param name="model">new model</param>
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

        /// <summary>
        /// deserialize message and apply a new model for an entity
        /// </summary>
        /// <param name="entity">target entity</param>
        /// <param name="message">message</param>
        public void Deserialize(IDCLEntity entity, object message)
        {
            SetModel(entity, deserializer.Invoke(message));
        }

        /// <summary>
        /// check if entity contains component
        /// </summary>
        /// <param name="entity">target entity</param>
        /// <returns>true if entity contains this component</returns>
        public bool HasComponent(IDCLEntity entity)
        {
            return entities.ContainsKey(entity.entityId);
        }

        /// <summary>
        /// get component data for an entity
        /// </summary>
        /// <param name="entity">target entity</param>
        /// <returns>component data, including model</returns>
        public ECSComponentData<ModelType> Get(IDCLEntity entity)
        {
            if (entities.TryGetValue(entity.entityId, out ECSComponentData<ModelType> data))
            {
                return data;
            }
            return null;
        }

        /// <summary>
        /// get every component data for every entity containing this component
        /// </summary>
        /// <returns>enumerator to iterate through the component data</returns>
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