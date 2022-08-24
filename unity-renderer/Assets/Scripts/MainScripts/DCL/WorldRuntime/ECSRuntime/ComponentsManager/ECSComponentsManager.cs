using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class ECSComponentsManager
    {
        private readonly IReadOnlyDictionary<int, ECSComponentsFactory.ECSComponentBuilder> componentBuilders;
        internal readonly KeyValueSet<int, IECSComponent> loadedComponents = new KeyValueSet<int, IECSComponent>();
        internal readonly IList<IECSComponentsGroup> componentsGroups = new List<IECSComponentsGroup>();

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

            for (int i = 0; i < componentsGroups.Count; i++)
            {
                IECSComponentsGroup compGroup = componentsGroups[i];
                if (compGroup.Match(component) && compGroup.Match(scene, entity))
                {
                    compGroup.Add(scene, entity);
                }
            }

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
            component?.Deserialize(scene, entity, message);
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

            for (int i = 0; i < componentsGroups.Count; i++)
            {
                IECSComponentsGroup compGroup = componentsGroups[i];
                if (compGroup.Match(component))
                {
                    compGroup.Remove(entity);
                }
            }

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

            for (int i = 0; i < componentsGroups.Count; i++)
            {
                componentsGroups[i].Remove(entity);
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
    }
}