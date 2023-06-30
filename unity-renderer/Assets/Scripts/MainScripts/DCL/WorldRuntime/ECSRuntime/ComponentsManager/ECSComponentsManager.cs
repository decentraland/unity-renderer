using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public class ECSComponentsManager
    {
        internal readonly KeyValueSet<int, IECSComponent> loadedComponents = new KeyValueSet<int, IECSComponent>();
        internal readonly IList<IECSComponentsGroup> componentsGroups = new List<IECSComponentsGroup>();
        internal readonly Dictionary<IDCLEntity, List<IECSComponentsGroup>> entitiesGroups =
            new Dictionary<IDCLEntity, List<IECSComponentsGroup>>();
        private readonly IReadOnlyDictionary<int, ECSComponentsFactory.ECSComponentBuilder> componentBuilders;

        public ECSComponentsManager(IReadOnlyDictionary<int, ECSComponentsFactory.ECSComponentBuilder> componentBuilders)
        {
            this.componentBuilders = componentBuilders;
        }

        /// <summary>
        /// get a component instance using it id
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns>component instance of null if it does not exist</returns>
        public IECSComponent GetComponent(int componentId)
        {
            loadedComponents.TryGetValue(componentId, out IECSComponent component);
            return component;
        }

        /// <summary>
        /// get or create a component
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns>the instance of existing or newly created component</returns>
        public IECSComponent GetOrCreateComponent(int componentId)
        {
            if (loadedComponents.TryGetValue(componentId, out IECSComponent component))
                return component;

            if (!componentBuilders.TryGetValue(componentId, out ECSComponentsFactory.ECSComponentBuilder componentBuilder))
                return null;

            component = componentBuilder.Invoke();
            loadedComponents.Add(componentId, component);

            return component;
        }

        /// <summary>
        /// get or create a component for an entity
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="scene"></param>
        /// <param name="entity"></param>
        /// <returns>the instance of existing or newly created component</returns>
        public IECSComponent GetOrCreateComponent(int componentId, IParcelScene scene, IDCLEntity entity)
        {
            IECSComponent component = GetOrCreateComponent(componentId);

            if (component == null || component.HasComponent(scene, entity))
                return component;

            component.Create(scene, entity);
            SetGroupsOnComponentCreated(component, scene, entity, componentsGroups, entitiesGroups);
            return component;
        }

        /// <summary>
        /// deserialize data for a component. it will create the component if it does not exists
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="scene"></param>
        /// <param name="entity"></param>
        /// <param name="message"></param>
        public void DeserializeComponent(int componentId, IParcelScene scene, IDCLEntity entity, object message)
        {
            var component = GetOrCreateComponent(componentId, scene, entity);

            if (component != null)
            {
                component.Deserialize(scene, entity, message);
                UpdateComponentGroups(component, scene, entity, entitiesGroups);
            }
        }

        public void SignalComponentCreated(IParcelScene scene, IDCLEntity entity, IECSComponent component)
        {
            SetGroupsOnComponentCreated(component, scene, entity, componentsGroups, entitiesGroups);
        }

        public void SignalComponentUpdated(IParcelScene scene, IDCLEntity entity, IECSComponent component)
        {
            UpdateComponentGroups(component, scene, entity, entitiesGroups);
        }

        /// <summary>
        /// remove a component from an entity
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="scene"></param>
        /// <param name="entity"></param>
        /// <returns>true if component removed successfully, false if entity didn't contain component</returns>
        public bool RemoveComponent(int componentId, IParcelScene scene, IDCLEntity entity)
        {
            if (!loadedComponents.TryGetValue(componentId, out IECSComponent component))
                return false;

            if (!component.Remove(scene, entity))
                return false;

            SetGroupsOnComponentRemoved(component, scene, entity, componentsGroups, entitiesGroups);

            return true;
        }

        /// <summary>
        /// remove all components of a given entity
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="entity"></param>
        public void RemoveAllComponents(IParcelScene scene, IDCLEntity entity)
        {
            int count = loadedComponents.Count;

            for (int i = 0; i < count; i++)
            {
                loadedComponents.Pairs[i].value.Remove(scene, entity);
            }

            if (entitiesGroups.TryGetValue(entity, out List<IECSComponentsGroup> entityInGroups))
            {
                for (int i = 0; i < entityInGroups.Count; i++)
                {
                    entityInGroups[i].Remove(entity);
                }

                entitiesGroups.Remove(entity);
            }
        }

        /// <summary>
        /// get if entity has any component
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool HasAnyComponent(IParcelScene scene, IDCLEntity entity)
        {
            int count = loadedComponents.Count;

            for (int i = 0; i < count; i++)
            {
                if (loadedComponents.Pairs[i].value.HasComponent(scene, entity))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// creates a components group
        /// </summary>
        /// <param name="componentId1">id of one of the components</param>
        /// <param name="componentId2">id of the other component</param>
        /// <typeparam name="T1">model type of `componentId1`</typeparam>
        /// <typeparam name="T2">model type of `componentId2`</typeparam>
        /// <returns></returns>
        public IECSReadOnlyComponentsGroup<T1, T2> CreateComponentGroup<T1, T2>(int componentId1, int componentId2)
        {
            var compGroup = new ECSComponentsGroup<T1, T2>(GetOrCreateComponent(componentId1), GetOrCreateComponent(componentId2));
            componentsGroups.Add(compGroup);
            return compGroup;
        }

        /// <summary>
        /// creates a components group
        /// </summary>
        /// <param name="componentId1">id of one of the components</param>
        /// <param name="componentId2">id of the other component</param>
        /// <param name="componentId3">id of the other component</param>
        /// <typeparam name="T1">model type of `componentId1`</typeparam>
        /// <typeparam name="T2">model type of `componentId2`</typeparam>
        /// <typeparam name="T3">model type of `componentId3`</typeparam>
        /// <returns></returns>
        public IECSReadOnlyComponentsGroup<T1, T2, T3> CreateComponentGroup<T1, T2, T3>(int componentId1, int componentId2, int componentId3)
        {
            var compGroup = new ECSComponentsGroup<T1, T2, T3>(
                GetOrCreateComponent(componentId1),
                GetOrCreateComponent(componentId2),
                GetOrCreateComponent(componentId3));

            componentsGroups.Add(compGroup);
            return compGroup;
        }

        /// <summary>
        /// creates a components group of components without `excludeComponentId`
        /// </summary>
        /// <param name="componentId">id of component that must be present</param>
        /// <param name="excludeComponentId">id of component that must not be present</param>
        /// <typeparam name="T1">type of component that must be present</typeparam>
        /// <returns></returns>
        public IECSReadOnlyComponentsGroup<T1> CreateComponentGroupWithoutComponent<T1>(int componentId, int excludeComponentId)
        {
            var compGroup = new ECSComponentsGroupWithout<T1>(GetOrCreateComponent(componentId),
                GetOrCreateComponent(excludeComponentId));

            componentsGroups.Add(compGroup);
            return compGroup;
        }

        /// <summary>
        /// creates a components group of components without `excludeComponentId`
        /// </summary>
        /// <param name="componentId1">id of component that must be present</param>
        /// <param name="componentId2">id of component that must be present</param>
        /// <param name="excludeComponentId">id of component that must not be present</param>
        /// <typeparam name="T1">model type of `componentId1`</typeparam>
        /// <typeparam name="T2">model type of `componentId2`</typeparam>
        /// <returns></returns>
        public IECSReadOnlyComponentsGroup<T1, T2> CreateComponentGroupWithoutComponent<T1, T2>(int componentId1, int componentId2, int excludeComponentId)
        {
            var compGroup = new ECSComponentsGroupWithout<T1, T2>(
                GetOrCreateComponent(componentId1),
                GetOrCreateComponent(componentId2),
                GetOrCreateComponent(excludeComponentId));

            componentsGroups.Add(compGroup);
            return compGroup;
        }

        private static void SetGroupsOnComponentCreated(IECSComponent component, IParcelScene scene, IDCLEntity entity,
            IList<IECSComponentsGroup> componentsGroups, IDictionary<IDCLEntity, List<IECSComponentsGroup>> entitiesGroups)
        {
            List<IECSComponentsGroup> entityInGroups = null;

            if (entitiesGroups.TryGetValue(entity, out entityInGroups))
            {
                for (int i = entityInGroups.Count - 1; i >= 0; i--)
                {
                    IECSComponentsGroup compGroup = entityInGroups[i];

                    if (compGroup.ShouldRemoveOnComponentAdd(component))
                    {
                        compGroup.Remove(entity);
                        entityInGroups.RemoveAt(i);
                    }
                }

                if (entityInGroups.Count == 0)
                {
                    entitiesGroups.Remove(entity);
                    entityInGroups = null;
                }
            }

            for (int i = 0; i < componentsGroups.Count; i++)
            {
                IECSComponentsGroup compGroup = componentsGroups[i];

                if (compGroup.ShouldAddOnComponentAdd(component) && compGroup.MatchEntity(scene, entity))
                {
                    compGroup.Add(scene, entity);

                    if (entityInGroups == null)
                    {
                        entityInGroups = new List<IECSComponentsGroup>();
                        entitiesGroups[entity] = entityInGroups;
                    }

                    entityInGroups.Add(compGroup);
                }
            }
        }

        private static void SetGroupsOnComponentRemoved(IECSComponent component, IParcelScene scene, IDCLEntity entity,
            IList<IECSComponentsGroup> componentsGroups, Dictionary<IDCLEntity, List<IECSComponentsGroup>> entitiesGroups)
        {
            List<IECSComponentsGroup> entityInGroups = null;

            if (entitiesGroups.TryGetValue(entity, out entityInGroups))
            {
                for (int i = entityInGroups.Count - 1; i >= 0; i--)
                {
                    IECSComponentsGroup compGroup = entityInGroups[i];

                    if (compGroup.ShouldRemoveOnComponentRemove(component))
                    {
                        compGroup.Remove(entity);
                        entityInGroups.RemoveAt(i);
                    }
                }

                if (entityInGroups.Count == 0)
                {
                    entitiesGroups.Remove(entity);
                    entityInGroups = null;
                }
            }

            for (int i = 0; i < componentsGroups.Count; i++)
            {
                IECSComponentsGroup compGroup = componentsGroups[i];

                if (compGroup.ShouldAddOnComponentRemove(component) && compGroup.MatchEntity(scene, entity))
                {
                    compGroup.Add(scene, entity);

                    if (entityInGroups == null)
                    {
                        entityInGroups = new List<IECSComponentsGroup>();
                        entitiesGroups[entity] = entityInGroups;
                    }

                    entityInGroups.Add(compGroup);
                }
            }
        }

        private static void UpdateComponentGroups(IECSComponent component, IParcelScene scene, IDCLEntity entity,
            IDictionary<IDCLEntity, List<IECSComponentsGroup>> entitiesGroups)
        {
            if (entitiesGroups.TryGetValue(entity, out var entityInGroups))
            {
                for (int i = 0; i < entityInGroups.Count; i++)
                {
                    IECSComponentsGroup compGroup = entityInGroups[i];
                    compGroup.Update(scene, entity, component);
                }
            }
        }
    }
}
