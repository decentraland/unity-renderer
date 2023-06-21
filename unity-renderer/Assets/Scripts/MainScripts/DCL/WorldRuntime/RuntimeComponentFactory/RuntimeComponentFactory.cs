using System;
using System.Collections.Generic;
using DCL.Components;
using UnityEngine;
using static DCL.IRuntimeComponentFactory;

namespace DCL
{
    public class RuntimeComponentFactory : IRuntimeComponentFactory
    {
        private readonly Dictionary<int, Func<IComponent>> builders = new ();

        public Dictionary<int, CreateCondition> createConditions { get; } = new ();
        public Dictionary<int, CreateOverride> createOverrides { get; } = new ();

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void RegisterBuilder(int classId, Func<IComponent> builder)
        {
            builders[classId] = builder;
        }

        public void UnregisterBuilder(int classId)
        {
            if (builders.ContainsKey(classId))
                builders.Remove(classId);
        }

        public IComponent CreateComponent(int classId)
        {
            if (builders.ContainsKey(classId))
                return builders[classId]();

            Debug.LogError($"Unknown classId: {classId} - Make sure the component is registered! (You forgot to add a plugin?)");
            return null;
        }
    }
}
