using System;
using System.Collections.Generic;
using DCL.Controllers;

namespace DCL.ECSRuntime
{
    public class ECSComponentsFactory
    {
        public delegate IECSComponent ECSComponentBuilder(IParcelScene scene);

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
            return (scene) => BuildComponent(scene, deserializer, handlerBuilder);
        }

        private static IECSComponent BuildComponent<ModelType>(
            IParcelScene scene,
            Func<object, ModelType> deserializer,
            Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            return new ECSComponent<ModelType>(scene, deserializer, handlerBuilder);
        }
    }
}