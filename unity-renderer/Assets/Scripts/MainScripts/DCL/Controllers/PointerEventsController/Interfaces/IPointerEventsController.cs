using System;
using UnityEngine;

namespace DCL
{
    public interface IPointerEventsController : IService
    {
        void Update();
        Ray GetRayFromCamera();
    }
}