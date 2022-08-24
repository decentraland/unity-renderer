using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public class ECSComponent<ModelType> : IECSComponent
    {
        internal readonly Dictionary<IParcelScene, Dictionary<long, ECSComponentData<ModelType>>> componentData =
            new Dictionary<IParcelScene, Dictionary<long, ECSComponentData<ModelType>>>();

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

            if (ComponentDataContainsEntity(componentData, scene, entity))
            {
                Debug.LogError($"entity {entityId} already contains component {typeof(ModelType)}", entity.gameObject);
                return;
            }

            var handler = handlerBuilder?.Invoke();

            ComponentDataAdd(componentData, scene, entity,
                new ECSComponentData<ModelType>()
                {
                    entity = entity,
                    model = default,
                    scene = scene,
                    handler = handler
                }
            );

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
            if (!ComponentDataRemove(componentData, scene, entity, out ECSComponentData<ModelType> data))
                return false;

            data.handler?.OnComponentRemoved(scene, entity);
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
            if (ComponentDataTryGet(componentData, scene, entity, out ECSComponentData<ModelType> data))
            {
                data.model = model;
                data.handler?.OnComponentModelUpdated(scene, entity, model);
            }
            else
            {
                Debug.LogError($"trying to update model but entity {entity.entityId} does not contains component {typeof(ModelType)}",
                    entity.gameObject);
            }
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
            return ComponentDataContainsEntity(componentData, scene, entity);
        }

        /// <summary>
        /// get component data for an entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        /// <returns>component data, including model</returns>
        public IECSReadOnlyComponentData<ModelType> Get(IParcelScene scene, IDCLEntity entity)
        {
            if (ComponentDataTryGet(componentData, scene, entity, out ECSComponentData<ModelType> data))
            {
                return data;
            }
            return null;
        }

        private static bool ComponentDataContainsEntity(IReadOnlyDictionary<IParcelScene, Dictionary<long, ECSComponentData<ModelType>>> componentData,
            IParcelScene scene, IDCLEntity entity)
        {
            if (!componentData.TryGetValue(scene, out Dictionary<long, ECSComponentData<ModelType>> entitiesData))
                return false;

            return entitiesData.ContainsKey(entity.entityId);
        }

        private static void ComponentDataAdd(IDictionary<IParcelScene, Dictionary<long, ECSComponentData<ModelType>>> componentData,
            IParcelScene scene, IDCLEntity entity, ECSComponentData<ModelType> newData)
        {
            if (!componentData.TryGetValue(scene, out Dictionary<long, ECSComponentData<ModelType>> entitiesData))
            {
                entitiesData = new Dictionary<long, ECSComponentData<ModelType>>();
                componentData.Add(scene, entitiesData);
            }
            entitiesData.Add(entity.entityId, newData);
        }

        private static bool ComponentDataRemove(IDictionary<IParcelScene, Dictionary<long, ECSComponentData<ModelType>>> componentData,
            IParcelScene scene, IDCLEntity entity, out ECSComponentData<ModelType> data)
        {
            if (!componentData.TryGetValue(scene, out Dictionary<long, ECSComponentData<ModelType>> entitiesData))
            {
                data = null;
                return false;
            }

            if (!entitiesData.TryGetValue(entity.entityId, out data))
                return false;

            entitiesData.Remove(entity.entityId);

            if (entitiesData.Count == 0)
            {
                componentData.Remove(scene);
            }
            return true;
        }

        private static bool ComponentDataTryGet(IReadOnlyDictionary<IParcelScene, Dictionary<long, ECSComponentData<ModelType>>> componentData,
            IParcelScene scene, IDCLEntity entity, out ECSComponentData<ModelType> data)
        {
            if (componentData.TryGetValue(scene, out Dictionary<long, ECSComponentData<ModelType>> entitiesData))
                return entitiesData.TryGetValue(entity.entityId, out data);

            data = null;
            return false;
        }
    }
}