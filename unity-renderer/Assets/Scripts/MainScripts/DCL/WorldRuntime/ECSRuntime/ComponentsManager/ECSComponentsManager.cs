using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class ECSComponentsManager
    {
        private readonly IReadOnlyDictionary<int, ECSComponentsFactory.ECSComponentBuilder> componentBuilders;
        internal readonly KeyValueSet<int, IECSComponent> loadedComponents = new KeyValueSet<int, IECSComponent>();

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
            if (loadedComponents.TryGetValue(componentId, out IECSComponent component))
            {
                if (!component.HasComponent(scene, entity))
                {
                    component.Create(scene, entity);
                }
            }
            else if (componentBuilders.TryGetValue(componentId, out ECSComponentsFactory.ECSComponentBuilder componentBuilder))
            {
                component = componentBuilder.Invoke();
                loadedComponents.Add(componentId, component);
                component.Create(scene, entity);
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
            component.Deserialize(scene, entity, message);
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
            if (loadedComponents.TryGetValue(componentId, out IECSComponent component))
            {
                return component.Remove(scene, entity);
            }
            return false;
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
    }
}