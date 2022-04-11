using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class SceneComponentsManager
    {
        private readonly IReadOnlyDictionary<ComponentsId, ComponentsFactory.ECSComponentBuilder> components;
        private readonly IReadOnlyDictionary<ComponentsId, IECSComponent> sceneComponents;
        private readonly IParcelScene scene;

        public SceneComponentsManager(IParcelScene scene, IReadOnlyDictionary<ComponentsId, ComponentsFactory.ECSComponentBuilder> components)
        {
            this.components = components;
            this.scene = scene;
        }

        public IECSComponent GetOrCreateComponent(ComponentsId componentId, IDCLEntity entity)
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
            }
            return component;
        }

        public void DeserializeComponent(ComponentsId componentId, IDCLEntity entity, object message)
        {
            GetOrCreateComponent(componentId, entity).Deserialize(entity, message);
        }

        public bool RemoveComponent(ComponentsId componentId, IDCLEntity entity)
        {
            if (sceneComponents.TryGetValue(componentId, out IECSComponent component))
            {
                return component.Remove(entity);
            }
            return false;
        }
    }
}