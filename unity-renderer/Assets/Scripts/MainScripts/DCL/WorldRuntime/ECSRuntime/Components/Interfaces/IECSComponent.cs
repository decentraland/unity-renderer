using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSComponent
    {
        /// <summary>
        /// creates and add component to an entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        void Create(IParcelScene scene, IDCLEntity entity);

        /// <summary>
        /// remove component from entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        /// <returns>true if component removed successfully, false if entity didn't contain component</returns>
        bool Remove(IParcelScene scene, IDCLEntity entity);

        /// <summary>
        /// deserialize message and apply a new model for an entity
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        /// <param name="message">message</param>
        void Deserialize(IParcelScene scene, IDCLEntity entity, object message);

        /// <summary>
        /// check if entity contains component
        /// </summary>
        /// <param name="scene">target scene</param>
        /// <param name="entity">target entity</param>
        /// <returns>true if entity contains this component</returns>
        bool HasComponent(IParcelScene scene, IDCLEntity entity);
    }
}