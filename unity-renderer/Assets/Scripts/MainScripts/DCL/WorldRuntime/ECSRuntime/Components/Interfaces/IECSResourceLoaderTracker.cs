using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSRuntime
{
    public interface IECSResourceLoaderTracker
    {
        event Action<IECSResourceLoaderTracker> OnResourceReady;
    }
}

