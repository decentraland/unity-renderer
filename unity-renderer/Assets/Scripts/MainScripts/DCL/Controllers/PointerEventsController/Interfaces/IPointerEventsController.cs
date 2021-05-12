using UnityEngine;

namespace DCL
{
    public interface IPointerEventsController
    {
        void Initialize();
        void Update();
        void Cleanup();
        Ray GetRayFromCamera();
    }
}