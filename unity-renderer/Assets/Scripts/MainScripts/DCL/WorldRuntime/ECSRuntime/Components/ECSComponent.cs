using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public class ECSComponent<ModelType> : IECSComponent
    {
        internal readonly DualKeyValueSet<IParcelScene, long, ECSComponentData<ModelType>> componentData =
            new DualKeyValueSet<IParcelScene, long, ECSComponentData<ModelType>>(50);

        private readonly Func<IECSComponentHandler<ModelType>> handlerBuilder;
        private readonly Func<object, ModelType> deserializer;

        public ECSComponent(Func<object, ModelType> deserializer, Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            this.deserializer = deserializer;
            this.handlerBuilder = handlerBuilder;
        }

        /// <summary>
        /// creates and add component to an entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        public void Create(IParcelScene scene, IDCLEntity entity)
        {
            var entityId = entity.entityId;

            if (componentData.ContainsKey(scene, entityId))
            {
                Debug.LogError($"entity {entityId.ToString()} already contains component {typeof(ModelType)}", entity.gameObject);
                return;
            }

            var handler = handlerBuilder?.Invoke();

            componentData.Add(scene, entityId, new ECSComponentData<ModelType>
            (
                entity: entity,
                model: default,
                scene: scene,
                handler: handler
            ));

            handler?.OnComponentCreated(scene, entity);
        }

        /// <summary>
        /// remove component from entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        /// <returns>true if component removed successfully, false if entity didn't contain component</returns>
        public bool Remove(IParcelScene scene, IDCLEntity entity)
        {
            if (!componentData.TryGetValue(scene, entity.entityId, out ECSComponentData<ModelType> data))
                return false;

            data.handler?.OnComponentRemoved(scene, entity);
            componentData.Remove(scene, entity.entityId);
            return true;
        }

        /// <summary>
        /// set component model for entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        /// <param name="model">new model</param>
        public void SetModel(IParcelScene scene, IDCLEntity entity, ModelType model)
        {
            SetModel(scene, entity.entityId, model);
        }

        /// <summary>
        /// set component model for entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entityId">target entity</param>
        /// <param name="model">new model</param>
        public void SetModel(IParcelScene scene, long entityId, ModelType model)
        {
            if (!componentData.TryGetValue(scene, entityId, out ECSComponentData<ModelType> data))
            {
                Debug.LogError($"trying to update model but entity {entityId.ToString()} does not contains component {typeof(ModelType)}");
                return;
            }

            componentData[scene, entityId] = data.With(model);
            data.handler?.OnComponentModelUpdated(scene, data.entity, model);
        }

        /// <summary>
        /// deserialize message and apply a new model for an entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        /// <param name="message">message</param>
        public void Deserialize(IParcelScene scene, IDCLEntity entity, object message)
        {
            SetModel(scene, entity, deserializer.Invoke(message));
        }

        /// <summary>
        /// check if entity contains component
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        /// <returns>true if entity contains this component</returns>
        public bool HasComponent(IParcelScene scene, IDCLEntity entity)
        {
            return componentData.ContainsKey(scene, entity.entityId);
        }

        /// <summary>
        /// get component data for an entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entityId">target entity id</param>
        /// <param name="data">entity's component data</param>
        /// <returns>`true` if data is exists</returns>///
        public bool TryGet(IParcelScene scene, long entityId, out ECSComponentData<ModelType> data)
        {
            return componentData.TryGetValue(scene, entityId, out data);
        }

        /// <summary>
        /// get component data for an entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entityId">target entity id</param>
        /// <returns>entity's component data</returns>///
        public ECSComponentData<ModelType>? Get(IParcelScene scene, long entityId)
        {
            if (componentData.TryGetValue(scene, entityId, out var data))
                return data;

            return null;
        }

        /// <summary>
        /// get component data for all entities
        /// </summary>
        /// <returns>list of component's data</returns>
        public IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<ModelType>>> Get()
        {
            return componentData.Pairs;
        }
    }
}
