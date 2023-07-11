using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface IAvatarsLODController : IService
    {
        void SetCamera(Camera screenshotCamera);

        Dictionary<string, IAvatarLODController> LodControllers { get; }
    }
}
