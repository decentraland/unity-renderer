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
        public Dictionary<int, IRuntimeComponentFactory.CreateCondition> createConditions { get; set; } =
            new Dictionary<int, IRuntimeComponentFactory.CreateCondition>();

        public Dictionary<int, IRuntimeComponentFactory.CreateOverride> createOverrides { get; set; } =
            new Dictionary<int, IRuntimeComponentFactory.CreateOverride>();
        
        protected delegate IComponent ComponentBuilder(int classId);

        protected Dictionary<int, ComponentBuilder> builders = new Dictionary<int, ComponentBuilder>();


        public IPoolableComponentFactory poolableComponentFactory { get; private set; }

        public void Initialize()
        {
            CoroutineStarter.Start(InitializeCoroutine());
        }

        IEnumerator InitializeCoroutine()
        {
            yield return null;
            poolableComponentFactory.PrewarmPools();
        }

        public void RegisterBuilder(int classId, Func<IComponent> builder)
        {
            if (builders.ContainsKey(classId))
                builders[classId] = (id) => builder();
            else
                builders.Add(classId, (id) => builder());
        }

        public void UnregisterBuilder(int classId)
        {
            if (!builders.ContainsKey(classId))
                return;

            builders.Remove(classId);
        }

        public RuntimeComponentFactory(IPoolableComponentFactory poolableComponentFactory = null)
        {
            this.poolableComponentFactory = poolableComponentFactory ?? PoolableComponentFactory.Create();
            CoroutineStarter.Start(InitializeCoroutine());
        }

        public IComponent CreateComponent(int classId)
        {
            if (!builders.ContainsKey(classId))
            {
                Debug.LogError($"Unknown classId");
                return null;
            }

            IComponent newComponent = builders[classId](classId);

            return newComponent;
        }

        public void Dispose()
        {
        }
    }
}