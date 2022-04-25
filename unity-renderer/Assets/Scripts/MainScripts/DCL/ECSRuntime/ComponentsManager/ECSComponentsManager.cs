using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public class ECSComponentsManager
    {
        private readonly IReadOnlyDictionary<int, ECSComponentsFactory.ECSComponentBuilder> componentBuilders;
        internal readonly Dictionary<int, IECSComponent> sceneComponents = new Dictionary<int, IECSComponent>();
        private readonly IParcelScene scene;

        public ECSComponentsManager(IParcelScene scene, IReadOnlyDictionary<int, ECSComponentsFactory.ECSComponentBuilder> componentBuilders)
        {
            this.componentBuilders = componentBuilders;
            this.scene = scene;
        }

        public IECSComponent GetComponent(int componentId)
        {
            sceneComponents.TryGetValue(componentId, out IECSComponent component);
            return component;
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
            else if (componentBuilders.TryGetValue(componentId, out ECSComponentsFactory.ECSComponentBuilder componentBuilder))
            {
                component = componentBuilder.Invoke(scene);
                sceneComponents.Add(componentId, component);
                component.Create(entity);
            }
            return component;
        }

        public void DeserializeComponent(int componentId, IDCLEntity entity, object message)
        {
            var component = GetOrCreateComponent(componentId, entity);
            component?.Deserialize(entity, message);
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