using System;
using System.Collections.Generic;
using DCL.Controllers;

namespace DCL.ECSRuntime
{
    public static class ComponentsFactory
    {
        public delegate IECSComponent ECSComponentBuilder(IParcelScene scene);

        private static Dictionary<ComponentsId, ECSComponentBuilder> components =
            new Dictionary<ComponentsId, ECSComponentBuilder>();

        public static IReadOnlyDictionary<ComponentsId, ECSComponentBuilder> definedComponents => components;

        public static void AddOrReplaceComponent<ModelType>(
            ComponentsId componentId,
            Func<object, ModelType> deserializer,
            Func<IComponentHandler<ModelType>> handlerBuilder)
        {
            components[componentId] = (scene) => DefineComponent(scene, deserializer, handlerBuilder);
        }

        public static IECSComponent DefineComponent<ModelType>(
            IParcelScene scene,
            Func<object, ModelType> deserializer,
            Func<IComponentHandler<ModelType>> handlerBuilder)
        {
            return new ECSComponent<ModelType>(scene, deserializer, handlerBuilder);
        }
    }
}