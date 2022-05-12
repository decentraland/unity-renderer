using System;
using System.Collections;
using System.Collections.Generic;
using DCL.ECSRuntime;
using UnityEngine;

public interface IECSComponentsManager
{
    /// <summary>
    /// Released when a new component has been added to the manager
    /// </summary>
    event Action<IECSComponent> OnComponentAdded;
}
