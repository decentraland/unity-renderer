using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class ECSComponentsManager : IECSComponentsManager
    {
        public event Action<IECSComponent> OnComponentAdded;
        
        private readonly IReadOnlyDictionary<int, ECSComponentsFactory.ECSComponentBuilder> componentBuilders;
        internal readonly Dictionary<int, IECSComponent> sceneComponents = new Dictionary<int, IECSComponent>();
        private readonly IParcelScene scene;

        public ECSComponentsManager(IParcelScene scene, IReadOnlyDictionary<int, ECSComponentsFactory.ECSComponentBuilder> componentBuilders)
        {
            this.componentBuilders = componentBuilders;
            this.scene = scene;
        }

        /// <summary>
        /// get a component instance using it id
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns>component instance of null if it does not exist</returns>
        public IECSComponent GetComponent(int componentId)
        {
            sceneComponents.TryGetValue(componentId, out IECSComponent component);
            return component;
        }

        /// <summary>
        /// get or create a component for an entity
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="entity"></param>
        /// <returns>the instance of existing or newly created component</returns>
        public IECSComponent GetOrCreateComponent(int componentId, IDCLEntity entity)
        {
            if (sceneComponents.TryGetValue(componentId, out IECSComponent component))
            {
                if (!component.HasComponent(entity))
                {
                    CreateComponent(entity);
                }
            }
            else if (componentBuilders.TryGetValue(componentId, out ECSComponentsFactory.ECSComponentBuilder componentBuilder))
            {
                component = componentBuilder.Invoke(scene);
                sceneComponents.Add(componentId, component);
                CreateComponent(entity);
            }
            return component;
        }

        /// <summary>
        /// deserialize data for a component. it will create the component if it does not exists
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="entity"></param>
        /// <param name="message"></param>
        public void DeserializeComponent(int componentId, IDCLEntity entity, object message)
        {
            var component = GetOrCreateComponent(componentId, entity);
            component?.Deserialize(entity, message);
        }

        /// <summary>
        /// remove a component from an entity
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="entity"></param>
        /// <returns>true if component removed successfully, false if entity didn't contain component</returns>
        public bool RemoveComponent(int componentId, IDCLEntity entity)
        {
            if (sceneComponents.TryGetValue(componentId, out IECSComponent component))
            {
                return component.Remove(entity);
            }
            return false;
        }

        /// <summary>
        /// remove all components of a given entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveAllComponents(IDCLEntity entity)
        {
            using (var iterator = sceneComponents.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.Remove(entity);
                }
            }
        }

        /// <summary>
        /// get if entity has any component
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool HasAnyComponent(IDCLEntity entity)
        {
            using (var iterator = sceneComponents.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.HasComponent(entity))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CreateComponent(IDCLEntity entity)
        {
            
        }
 
    }
}