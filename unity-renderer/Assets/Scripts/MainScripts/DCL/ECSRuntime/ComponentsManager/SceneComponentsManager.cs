using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class SceneComponentsManager
    {
        private readonly IReadOnlyDictionary<int, ComponentsFactory.ECSComponentBuilder> components;
        internal readonly Dictionary<int, IECSComponent> sceneComponents = new Dictionary<int, IECSComponent>();
        private readonly IParcelScene scene;

        public SceneComponentsManager(IParcelScene scene, IReadOnlyDictionary<int, ComponentsFactory.ECSComponentBuilder> components)
        {
            this.components = components;
            this.scene = scene;
        }

        public IECSComponent GetOrCreateComponent(int componentId, IDCLEntity entity)
        {
            if (sceneComponents.TryGetValue(componentId, out IECSComponent component))
            {
                if (!component.HasComponent(entity))
                {
                    component.Create(entity);
                }
            }
            else if (components.TryGetValue(componentId, out ComponentsFactory.ECSComponentBuilder componentBuilder))
            {
                component = componentBuilder.Invoke(scene);
                sceneComponents.Add(componentId, component);
                component.Create(entity);
            }
            return component;
        }

        public void DeserializeComponent(int componentId, IDCLEntity entity, object message)
        {
            GetOrCreateComponent(componentId, entity).Deserialize(entity, message);
        }

        public bool RemoveComponent(int componentId, IDCLEntity entity)
        {
            if (sceneComponents.TryGetValue(componentId, out IECSComponent component))
            {
                return component.Remove(entity);
            }
            return false;
        }
    }
}