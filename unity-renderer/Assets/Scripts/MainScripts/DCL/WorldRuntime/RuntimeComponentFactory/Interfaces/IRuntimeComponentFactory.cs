﻿using System;
using DCL.Components;

namespace DCL
{
    public interface IRuntimeComponentFactory : IService
    {
        IComponent CreateComponent(int classId);

        void RegisterComponentBuilder(int classId, Func<IComponent> builder);
    }
}