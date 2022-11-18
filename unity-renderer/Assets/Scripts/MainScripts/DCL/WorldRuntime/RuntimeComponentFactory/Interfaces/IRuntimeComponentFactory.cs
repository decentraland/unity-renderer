using System;
using System.Collections.Generic;
using DCL.Components;

namespace DCL
{
    public interface IRuntimeComponentFactory : IService
    {
        public delegate bool CreateCondition(int sceneNumber, int classId);

        public delegate void CreateOverride(int sceneNumber, long entityId, ref int classId, object data);

        Dictionary<int, CreateCondition> createConditions { get; }
        Dictionary<int, CreateOverride> createOverrides { get; }

        IComponent CreateComponent(int classId);

        void RegisterBuilder(int classId, Func<IComponent> builder);

        void UnregisterBuilder(int classId);
    }
}