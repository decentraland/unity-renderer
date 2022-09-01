using System;
using System.Collections.Generic;

namespace DCL.ECSRuntime
{
    public class ECSComponentsFactory
    {
        public delegate IECSComponent ECSComponentBuilder();

        private readonly Dictionary<int, ECSComponentBuilder> components =
            new Dictionary<int, ECSComponentBuilder>();

        /// <summary>
        /// read only dictionary containing components builders
        /// </summary>
        public IReadOnlyDictionary<int, ECSComponentBuilder> componentBuilders => components;

        /// <summary>
        /// put component to the components builder dictionary
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="deserializer"></param>
        /// <param name="handlerBuilder"></param>
        /// <typeparam name="ModelType"></typeparam>
        public void AddOrReplaceComponent<ModelType>(
            int componentId,
            Func<object, ModelType> deserializer,
            Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            components[componentId] = CreateComponentBuilder(deserializer, handlerBuilder);
        }

        /// <summary>
        /// Remove component to the components builder dictionary
        /// </summary>
        /// <param name="componentId"></param>
        public void RemoveComponent(int componentId)
        {
            components.Remove(componentId);
        }

        /// <summary>
        /// creates a component builder
        /// </summary>
        /// <param name="deserializer"></param>
        /// <param name="handlerBuilder"></param>
        /// <typeparam name="ModelType"></typeparam>
        /// <returns>component builder</returns>
        public static ECSComponentBuilder CreateComponentBuilder<ModelType>(
            Func<object, ModelType> deserializer,
            Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            return () => BuildComponent(deserializer, handlerBuilder);
        }

        private static IECSComponent BuildComponent<ModelType>(
            Func<object, ModelType> deserializer,
            Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            return new ECSComponent<ModelType>(deserializer, handlerBuilder);
        }
    }
}