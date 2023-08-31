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
        /// <param name="handlerBuilder"></param>
        /// <param name="deserializer"></param>
        /// <typeparam name="ModelType"></typeparam>
        public void AddOrReplaceComponent<ModelType>(int componentId,
            Func<IECSComponentHandler<ModelType>> handlerBuilder,
            Func<object, ModelType> deserializer)
        {
            components[componentId] = CreateComponentBuilder(handlerBuilder, deserializer);
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
        /// <param name="handlerBuilder"></param>
        /// <param name="deserializer"></param>
        /// <typeparam name="ModelType"></typeparam>
        /// <returns>component builder</returns>
        public static ECSComponentBuilder CreateComponentBuilder<ModelType>(Func<IECSComponentHandler<ModelType>> handlerBuilder, Func<object, ModelType> deserializer)
        {
            return () => BuildComponent(handlerBuilder, deserializer);
        }

        private static IECSComponent BuildComponent<ModelType>(Func<IECSComponentHandler<ModelType>> handlerBuilder, Func<object, ModelType> deserializer)
        {
            return new ECSComponent<ModelType>(deserializer, handlerBuilder);
        }

        // FD:: testing out the following methods for pooling support -----------------------------

        /// <summary>
        /// Add or replace a component in the component builder dictionary with pooling support.
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="componentPool"></param>
        /// <param name="handlerBuilder"></param>
        /// <typeparam name="ModelType"></typeparam>
        public void AddOrReplaceComponentWithPooling<ModelType>(
            int componentId,
            IComponentPool<ModelType> componentPool,
            Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            components[componentId] = CreatePoolableComponentBuilder(componentPool, handlerBuilder);
        }

        /// <summary>
        /// Creates a component builder with pooling support.
        /// </summary>
        /// <param name="componentPool"></param>
        /// <param name="handlerBuilder"></param>
        /// <typeparam name="ModelType"></typeparam>
        /// <returns>Returns a delegate to create the component.</returns>
        public static ECSComponentBuilder CreatePoolableComponentBuilder<ModelType>(
            IComponentPool<ModelType> componentPool,
            Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            return () => BuildPoolableComponent(componentPool, handlerBuilder);
        }

        /// <summary>
        /// Builds a component instance with pooling support.
        /// </summary>
        /// <param name="componentPool"></param>
        /// <param name="handlerBuilder"></param>
        /// <typeparam name="ModelType"></typeparam>
        /// <returns>Returns the constructed component.</returns>
        private static IECSComponent BuildPoolableComponent<ModelType>(
            IComponentPool<ModelType> componentPool,
            Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            return new ECSComponent<ModelType>(handlerBuilder, componentPool);
        }

    }
}
