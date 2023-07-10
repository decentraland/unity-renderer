using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public class RuntimeComponentFactory : IRuntimeComponentFactory
    {
        // Temporal delegate for special behaviours. Should be deleted or refactored later.
        public Dictionary<int, IRuntimeComponentFactory.CreateCondition> createConditions { get; } = new ();
        public Dictionary<int, IRuntimeComponentFactory.CreateOverride> createOverrides { get; } = new ();

        private readonly Dictionary<int, Func<IComponent>> builders = new ();

        public void RegisterBuilder(int classId, Func<IComponent> builder)
        {
            builders[classId] = builder;
        }

        public void UnregisterBuilder(int classId)
        {
            if (!builders.ContainsKey(classId))
                return;

            builders.Remove(classId);
        }

        public IComponent CreateComponent(int classId)
        {
            if (!builders.ContainsKey(classId))
            {
                Debug.LogError(
                    $"Unknown classId: {classId} - Make sure the component is registered! (You forgot to add a plugin?)");
                return null;
            }

            IComponent newComponent = builders[classId]();

            return newComponent;
        }

        public RuntimeComponentFactory()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
}
