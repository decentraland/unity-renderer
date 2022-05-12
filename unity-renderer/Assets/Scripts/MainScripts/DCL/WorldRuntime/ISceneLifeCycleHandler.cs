using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

public interface ISceneLifeCycleHandler
{
    event Action<ParcelScene> OnSceneReady;
    event Action<ParcelScene> OnStateRefreshed;
    bool isReady { get; }
}
