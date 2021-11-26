﻿using System;
using DCL.Rendering;

namespace DCL.Controllers
{
    public interface IWorldBlockersController : IDisposable
    {
        void Initialize(ISceneHandler sceneHandler, IBlockerInstanceHandler blockerInstanceHandler);
        void InitializeWithDefaultDependencies(ISceneHandler sceneHandler, ICullingController cullingController);
        void SetupWorldBlockers();
        void SetEnabled(bool targetValue);
    }
}