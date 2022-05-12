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
    /// This will get or create a component using its Id
    /// </summary>
    /// <param name="componentId"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    IECSComponent GetOrCreateComponent(int componentId, IDCLEntity entity);
}
