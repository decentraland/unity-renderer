using System.Collections.Generic;

namespace DCL.ECSRuntime
{
    public interface IECSReadOnlyComponentsGroupOfTwo<T1, T2>
    {
        IReadOnlyList<ECSComponentsGroupOfTwoData<T1, T2>> group { get; }
    }
}