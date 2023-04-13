using System.Collections.Generic;

namespace DCL.ECSRuntime
{
    public interface IECSReadOnlyComponentsGroup<T1>
    {
        IReadOnlyList<ECSComponentsGroupData<T1>> group { get; }
    }

    public interface IECSReadOnlyComponentsGroup<T1, T2>
    {
        IReadOnlyList<ECSComponentsGroupData<T1, T2>> group { get; }
    }

    public interface IECSReadOnlyComponentsGroup<T1, T2, T3>
    {
        IReadOnlyList<ECSComponentsGroupData<T1, T2, T3>> group { get; }
    }
}
