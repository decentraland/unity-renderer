using System;
using System.Collections;
using System.Collections.Generic;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

public interface IECSComponentsManager
{
    /// <summary>
    /// Released when a new component has been added to the manager
    /// </summary>
    event Action<IECSComponent> OnComponentAdded;

    /// <summary>
    /// deserialize data for a component. it will create the component if it does not exists
    /// </summary>
    /// <param name="componentId"></param>
    /// <param name="entity"></param>
    /// <param name="message"></param>
    void DeserializeComponent(int componentId, IDCLEntity entity, object message);

    /// <summary>
    /// Get if entity has any component
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    bool HasAnyComponent(IDCLEntity entity);

    /// <summary>
    /// Remove all components of a given entity
    /// </summary>
    /// <param name="entity"></param>
    void RemoveAllComponents(IDCLEntity entity);
    
    /// <summary>
    /// Remove a component from an entity
    /// </summary>
    /// <param name="componentId"></param>
    /// <param name="entity"></param>
    /// <returns>true if component removed successfully, false if entity didn't contain component</returns>
    bool RemoveComponent(int componentId, IDCLEntity entity);

    /// <summary>
    /// This will get or create a component using its Id
    /// </summary>
    /// <param name="componentId"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    IECSComponent GetOrCreateComponent(int componentId, IDCLEntity entity);
}
