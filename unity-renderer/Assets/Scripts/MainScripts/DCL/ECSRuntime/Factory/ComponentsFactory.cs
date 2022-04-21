using System;
using System.Collections.Generic;
using DCL.Controllers;

namespace DCL.ECSRuntime
{
    public static class ComponentsFactory
    {
        public delegate IECSComponent ECSComponentBuilder(IParcelScene scene);

        private static Dictionary<int, ECSComponentBuilder> components =
            new Dictionary<int, ECSComponentBuilder>();

        public static IReadOnlyDictionary<int, ECSComponentBuilder> definedComponents => components;

        public static void AddOrReplaceComponent<ModelType>(
            int componentId,
            Func<object, ModelType> deserializer,
            Func<IComponentHandler<ModelType>> handlerBuilder)
        {
            components[componentId] = (scene) => BuildComponent(scene, deserializer, handlerBuilder);
        }

        private static IECSComponent BuildComponent<ModelType>(
            IParcelScene scene,
            Func<object, ModelType> deserializer,
            Func<IComponentHandler<ModelType>> handlerBuilder)
        {
            return new ECSComponent<ModelType>(scene, deserializer, handlerBuilder);
        }

        public static ECSComponentBuilder DefineComponent<ModelType>(
            Func<object, ModelType> deserializer,
            Func<IComponentHandler<ModelType>> handlerBuilder)
        {
            return (scene) => BuildComponent(scene, deserializer, handlerBuilder);
        }
    }
}