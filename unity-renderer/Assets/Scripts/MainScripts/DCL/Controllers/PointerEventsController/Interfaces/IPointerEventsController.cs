using System;
using UnityEngine;

namespace DCL
{
    public interface IPointerEventsController : IDisposable
    {
        void Update();
        Ray GetRayFromCamera();
    }
}