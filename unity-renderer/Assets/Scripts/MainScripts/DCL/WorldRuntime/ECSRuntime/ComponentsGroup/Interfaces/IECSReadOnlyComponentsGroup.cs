using System.Collections.Generic;

namespace DCL.ECSRuntime
{
    public interface IECSReadOnlyComponentsGroup<T1, T2>
    {
        IReadOnlyList<ECSComponentsGroupData<T1, T2>> group { get; }
    }
}