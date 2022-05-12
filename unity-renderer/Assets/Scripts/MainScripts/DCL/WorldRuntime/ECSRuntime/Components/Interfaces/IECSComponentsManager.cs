using System;
using System.Collections;
using System.Collections.Generic;
using DCL.ECSRuntime;
using UnityEngine;

public interface IECSComponentsManager
{
    event Action<IECSComponent> OnComponentAdded;
}
