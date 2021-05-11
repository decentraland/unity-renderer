using UnityEngine;

namespace DCL.Components
{
    public interface IDelayedComponent : IComponent, ICleanable
    {
        CustomYieldInstruction yieldInstruction { get; }
        Coroutine routine { get; }
        bool isRoutineRunning { get; }
    }
}