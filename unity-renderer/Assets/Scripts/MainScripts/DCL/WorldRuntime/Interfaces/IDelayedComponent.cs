using UnityEngine;

namespace DCL.Components
{
    public interface IDelayedComponent : IComponent, ICleanable
    {
        WaitForComponentUpdate yieldInstruction { get; }
        Coroutine routine { get; }
        bool isRoutineRunning { get; }
    }
}