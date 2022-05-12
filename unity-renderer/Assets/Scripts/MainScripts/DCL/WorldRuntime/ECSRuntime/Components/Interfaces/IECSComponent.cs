using System;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSComponent
    {
        event Action<IECSComponent> OnComponentReady;
        
        /// <summary>
        /// creates and add component to an entity
        /// </summary>
        /// <param name="entity">target entity</param>
        void Create(IDCLEntity entity);
        
        /// <summary>
        /// remove component from entity
        /// </summary>
        /// <param name="entity">target entity</param>
        /// <returns>true if component removed successfully, false if entity didn't contain component</returns>
        bool Remove(IDCLEntity entity);
        
        /// <summary>
        /// deserialize message and apply a new model for an entity
        /// </summary>
        /// <param name="entity">target entity</param>
        /// <param name="message">message</param>
        void Deserialize(IDCLEntity entity, object message);
        
        /// <summary>
        /// check if entity contains component
        /// </summary>
        /// <param name="entity">target entity</param>
        /// <returns>true if entity contains this component</returns>
        bool HasComponent(IDCLEntity entity);
    }
}