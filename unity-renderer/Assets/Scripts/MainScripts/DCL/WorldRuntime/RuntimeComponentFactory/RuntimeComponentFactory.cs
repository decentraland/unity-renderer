﻿using System;
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

        public IComponent CreateComponent(int classId)
        {
            if (!builders.ContainsKey(classId))
            {
                Debug.LogError(
                    $"Unknown classId: {classId} - Make sure the component is registered! (You forgot to add a plugin?)");
                return null;
            }

            IComponent newComponent = builders[classId](classId);

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